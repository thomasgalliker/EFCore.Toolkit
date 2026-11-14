using EFCore.Toolkit.Extensions;

using System.Linq.Expressions;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Xunit;

namespace EFCore.Toolkit.Tests.Extensions
{
    [Trait(Traits.Category, Traits.UnitTests)]
    public class ModelBuilderExtensionsTests
    {
        [Fact]
        public void ApplyQueryFilter_EntityImplementsInterface_AddsDeclaredQueryFilter()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();

            // Act
            modelBuilder.ApplyQueryFilter<IFilterableEntity>(entity => !entity.IsDeleted);

            // Assert
            var filterableEntityType = modelBuilder.Model.FindEntityType(typeof(FilterableEntity));
            var queryFilter = filterableEntityType!.GetDeclaredQueryFilters()
                .Should()
                .ContainSingle(filter => filter.Key == typeof(IFilterableEntity).FullName)
                .Subject;

            IsMatch(queryFilter.Expression!, new FilterableEntity { IsDeleted = false }).Should().BeTrue();
            IsMatch(queryFilter.Expression!, new FilterableEntity { IsDeleted = true }).Should().BeFalse();
        }

        [Fact]
        public void ApplyQueryFilter_EntityDoesNotImplementInterface_DoesNotAddDeclaredQueryFilter()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();

            // Act
            modelBuilder.ApplyQueryFilter<IFilterableEntity>(entity => !entity.IsDeleted);

            // Assert
            var unfilteredEntityType = modelBuilder.Model.FindEntityType(typeof(UnfilteredEntity));
            unfilteredEntityType!.GetDeclaredQueryFilters().Should().BeEmpty();
        }

        [Fact]
        public void ApplyQueryFilter_SameInterfaceAppliedTwice_CombinesDeclaredQueryFilter()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();

            // Act
            modelBuilder.ApplyQueryFilter<IFilterableEntity>(entity => !entity.IsDeleted);
            modelBuilder.ApplyQueryFilter<IFilterableEntity>(entity => entity.TenantId == "tenant-1");

            // Assert
            var filterableEntityType = modelBuilder.Model.FindEntityType(typeof(FilterableEntity));
            var queryFilter = filterableEntityType!.GetDeclaredQueryFilters()
                .Should()
                .ContainSingle(filter => filter.Key == typeof(IFilterableEntity).FullName)
                .Subject;

            IsMatch(queryFilter.Expression!, new FilterableEntity { IsDeleted = false, TenantId = "tenant-1" }).Should().BeTrue();
            IsMatch(queryFilter.Expression!, new FilterableEntity { IsDeleted = true, TenantId = "tenant-1" }).Should().BeFalse();
            IsMatch(queryFilter.Expression!, new FilterableEntity { IsDeleted = false, TenantId = "tenant-2" }).Should().BeFalse();
        }

        private static ModelBuilder CreateModelBuilder()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            modelBuilder.Entity<FilterableEntity>();
            modelBuilder.Entity<UnfilteredEntity>();

            return modelBuilder;
        }

        private static bool IsMatch(LambdaExpression queryFilter, FilterableEntity entity)
        {
            return (bool)queryFilter.Compile().DynamicInvoke(entity)!;
        }

        private interface IFilterableEntity
        {
            bool IsDeleted { get; set; }

            string? TenantId { get; set; }
        }

        private sealed class FilterableEntity : IFilterableEntity
        {
            public int Id { get; set; }

            public bool IsDeleted { get; set; }

            public string? TenantId { get; set; }
        }

        private sealed class UnfilteredEntity
        {
            public int Id { get; set; }
        }
    }
}
