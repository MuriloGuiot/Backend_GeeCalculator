using GEE_Calculator.Application.Auth;
using GEE_Calculator.Infrastructure.Auth;
using GEE_Calculator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GeeCalculatorDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

        return services;
    }
}
