using GEE_Calculator.Application.Auth;
using GEE_Calculator.Infrastructure.Auth;
using GEE_Calculator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GeeCalculatorDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var keycloakSection = configuration.GetSection("Authentication:Keycloak");
                var authority = keycloakSection["Authority"];
                var audience = keycloakSection["Audience"];

                if (!string.IsNullOrWhiteSpace(authority))
                {
                    options.Authority = authority;
                }

                if (!string.IsNullOrWhiteSpace(audience))
                {
                    options.Audience = audience;
                }

                options.RequireHttpsMetadata = keycloakSection.GetValue("RequireHttpsMetadata", true);
                options.TokenValidationParameters.ValidateAudience = !string.IsNullOrWhiteSpace(audience);
            });
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

        return services;
    }
}
