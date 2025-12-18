using EFCore.Toolkit.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Toolkit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDbContextBase<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsBuilder = null) where TDbContext : DbContextBase
        {
            services.AddSingleton<IInterceptor, UpdateAuditableInterceptor>();
            services.AddSingleton<IInterceptor, SoftDeleteInterceptor>();

            services.AddDbContext<TDbContext>((s, ob) =>
            {
                var interceptors = s.GetServices<IInterceptor>().ToList();
                foreach (var interceptor in interceptors)
                {
                    ob.AddInterceptors(interceptors);
                }

                optionsBuilder?.Invoke(ob);
            });
        }
    }
}