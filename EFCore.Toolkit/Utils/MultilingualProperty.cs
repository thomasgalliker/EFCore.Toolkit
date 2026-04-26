using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EFCore.Toolkit.Utils
{
    public static class MultilingualProperty
    {
        public static T? GetCultureSpecificPropertyValue<T>(object source, [CallerMemberName] string? propertyName = null, CultureInfo? cultureInfo = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(propertyName);

            var sourceType = source.GetType();
            var propertyInfo = GetCultureSpecificProperty(source.GetType(), propertyName);
            if (propertyInfo != null)
            {
                return (T?)propertyInfo.GetValue(source);
            }

            throw new InvalidOperationException($"Property '{propertyName}' is not present on source type '{sourceType.Name}'");
        }

        public static void SetCultureSpecificPropertyValue<T>(object source, T value, [CallerMemberName] string? propertyName = null, CultureInfo? cultureInfo = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(propertyName);

            var sourceType = source.GetType();
            var propertyInfo = GetCultureSpecificProperty(sourceType, propertyName);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(source, value);
            }
            else
            {
                throw new InvalidOperationException($"Property '{propertyName}' is not present on source type '{sourceType.Name}'");
            }
        }

        private static PropertyInfo? GetCultureSpecificProperty(Type type, string propertyName, CultureInfo? cultureInfo = null)
        {
            cultureInfo ??= Thread.CurrentThread.CurrentUICulture;
            return type.GetProperty(propertyName + "_" + cultureInfo.TwoLetterISOLanguageName);
        }
    }
}