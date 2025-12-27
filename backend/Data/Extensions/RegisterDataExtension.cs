namespace Data.Extensions;

using Abstractions;
using EFCoreSecondLevelCacheInterceptor;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Providers;
using Seed;

public static class RegisterDataExtension {
    public static IServiceCollection AddPlannerData(this IServiceCollection services, IConfiguration configuration) {
        string? dbName = configuration["Database:Name"];

        if (string.IsNullOrWhiteSpace(dbName)) {
            dbName = "PlannerDb";
        }

        services.Configure<PlannerSnapshotOptions>(configuration.GetSection("PlannerSnapshot"));

        services.AddMemoryCache();
        services.AddSingleton<IPlannerSnapshotProvider, PlannerSnapshotProvider>();

        services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider().UseCacheKeyPrefix("PlannerEF_").UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1)));

        services.AddDbContext<DataContext>(
            (sp, options) => {
                options.UseInMemoryDatabase(dbName);

                options.AddInterceptors(sp.GetRequiredService<SecondLevelCacheInterceptor>());
            }
        );

        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.Scan(scan => scan.FromAssembliesOf(typeof(IBaseRepository<>)).AddClasses(c => c.AssignableTo(typeof(BaseRepository<>))).AsImplementedInterfaces().WithScopedLifetime());

        services.AddScoped<IDataSeeder, DataSeeder>();

        return services;
    }
}