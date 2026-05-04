using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Infrastructure.Persistence;

namespace GEE_Calculator;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "GEE Calculator API v1");
            options.RoutePrefix = "swagger";
        });

        app.MapGet("/", () => Results.Redirect("/swagger"));

        return app;
    }

    public static WebApplication UseTenantResolution(this WebApplication app)
    {
        app.UseMiddleware<TenantHeaderRequirementMiddleware>();
        return app;
    }

    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();

        return app;
    }
}
