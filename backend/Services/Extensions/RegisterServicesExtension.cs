namespace Services.Extensions;

using Domain.Models;
using Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services;

public static class RegisterServicesExtension {
    public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration configuration) {
        services.Configure<PlannerOptions>(configuration.GetSection("Planner"));

        services.AddScoped<IPlannerMetadataBuilder, PlannerMetadataBuilder>();
        services.AddScoped<IPlannerService, PlannerService>();

        return services;
    }
}