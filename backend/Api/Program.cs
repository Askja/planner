using Data.Extensions;
using Data.Interfaces;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Enums;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Logger.Extensions;
using Services.Extensions;
using Services.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.AddLogging();

builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory(new Container()));

builder.Services.AddPlannerData(builder.Configuration);
builder.Services.AddAllServices(builder.Configuration);

WebApplication app = builder.Build();

app.UseExceptionHandling();
app.UseLogging();

using (IServiceScope scope = app.Services.CreateScope()) {
    IDataSeeder seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync(app.Lifetime.ApplicationStopping);
}

RouteGroupBuilder plannerGroup = app.MapGroup("/api/planner");

plannerGroup.MapGet(
    "/", async (string[]? skuSubName, PlannerLevel[]? level, IPlannerService plannerService, CancellationToken ct) => {
        PlannerRowResponse response = await plannerService.GetPlannerAsync(skuSubName, level, ct);

        return Results.Ok(response);
    }
);

plannerGroup.MapPatch(
    "/", async (PlannerRowRequest request, IPlannerService plannerService, CancellationToken ct) => {
        await plannerService.UpdatePlanningAsync(request, ct);

        return Results.NoContent();
    }
);

app.MapGet(
    "/health", () => Results.Ok(
        new {
            status = "ok"
        }
    )
);

app.Run();