using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Migrations
{
    /// <summary>
    /// Generates migration identifiers that use an incrementing numeric prefix.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Migration identifiers are generated as <c>{version}_{name}</c>. The version is a numeric prefix
    /// that is incremented from the highest existing migration version for the same <typeparamref name="TContext" />.
    /// </para>
    /// <para>
    /// If no existing migration uses a numeric prefix, the first generated identifier starts at
    /// <c>00000000000001</c>. Existing sequential migrations keep their prefix width, so
    /// <c>000003_AddUsers</c> is followed by <c>000004_NextMigration</c>.
    /// </para>
    /// <para>
    /// Existing Entity Framework timestamp-style identifiers in the format <c>yyyyMMddHHmmss_Name</c>
    /// are treated as legacy numeric prefixes. When such an identifier is the highest existing prefix,
    /// the generator starts the next sequence at the next leading-digit range. For example,
    /// <c>20260122115839_InitialCreate</c> is followed by <c>30000000000000_NextMigration</c>.
    /// Subsequent migrations continue from that generated number.
    /// </para>
    /// </remarks>
    /// <typeparam name="TContext">The database context type the migrations belong to.</typeparam>
    public sealed class SequentialMigrationsIdGenerator<TContext> : IMigrationsIdGenerator where TContext : DbContext
    {
        private const int DefaultMinimumDigits = 14;

        private static readonly Regex VersionRegex = new(@"^(?<version>\d*[1-9]\d*)_(?<name>.+)$", RegexOptions.Compiled);
        private static readonly Regex LegacyTimestampRegex = new(@"^\d{14}_.+$", RegexOptions.Compiled);

        /// <inheritdoc />
        public string GenerateId(string name)
        {
            var migrationIds = typeof(TContext).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Migration)) &&
                            t.GetCustomAttribute<DbContextAttribute>()?.ContextType == typeof(TContext))
                .Select(t => t.GetCustomAttribute<MigrationAttribute>()?.Id)
                .Where(id => id != null)
                .Cast<string>()
                .ToArray();

            var currentSequence = migrationIds
                .Select(GetSequence)
                .Append(new MigrationSequence(0, DefaultMinimumDigits))
                .MaxBy(sequence => sequence.Version);

            var legacyTransitionSequence = migrationIds
                .Select(GetLegacyTimestampTransitionSequence)
                .Append(new MigrationSequence(0, DefaultMinimumDigits))
                .MaxBy(sequence => sequence.Version);

            MigrationSequence sequence = currentSequence.Version < legacyTransitionSequence.Version
                ? legacyTransitionSequence
                : currentSequence;

            return $"{(sequence.Version + 1).ToString($"D{sequence.MinimumDigits}")}_{name}";
        }

        /// <inheritdoc />
        public string GetName(string id)
        {
            int separatorIndex = id.IndexOf('_');
            return separatorIndex < 0 ? id : id[(separatorIndex + 1)..];
        }

        /// <inheritdoc />
        public bool IsValidId(string value)
        {
            return value != null && TryGetVersion(value, out _);
        }

        private static MigrationSequence GetSequence(string id)
        {
            return TryGetVersion(id, out long version, out int minimumDigits)
                ? new MigrationSequence(version, minimumDigits)
                : new MigrationSequence(0, DefaultMinimumDigits);
        }

        private static bool IsLegacyTimestampId(string id)
        {
            if (!LegacyTimestampRegex.IsMatch(id))
            {
                return false;
            }

            int separatorIndex = id.IndexOf('_');
            string timestamp = id[..separatorIndex];

            return DateTime.TryParseExact(
                timestamp,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        private static MigrationSequence GetLegacyTimestampTransitionSequence(string id)
        {
            if (!IsLegacyTimestampId(id) ||
                !TryGetVersion(id, out long version, out int minimumDigits))
            {
                return new MigrationSequence(0, DefaultMinimumDigits);
            }

            return new MigrationSequence(GetNextSequenceStart(version) - 1, minimumDigits);
        }

        private static long GetNextSequenceStart(long version)
        {
            long magnitude = 1;
            while (version / magnitude >= 10)
            {
                magnitude *= 10;
            }

            return ((version / magnitude) + 1) * magnitude;
        }

        private static bool TryGetVersion(string? id, out long version)
        {
            return TryGetVersion(id, out version, out _);
        }

        private static bool TryGetVersion(string? id, out long version, out int minimumDigits)
        {
            version = 0;
            minimumDigits = DefaultMinimumDigits;

            if (id == null)
            {
                return false;
            }

            var match = VersionRegex.Match(id);
            if (!match.Success ||
                !long.TryParse(match.Groups["version"].Value, out version))
            {
                return false;
            }

            minimumDigits = match.Groups["version"].Value.Length;
            return true;
        }

        private readonly record struct MigrationSequence(long Version, int MinimumDigits);
    }
}
