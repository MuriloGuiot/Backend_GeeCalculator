using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.Calculations;
using GEE_Calculator.Infrastructure.Auth;
using GEE_Calculator.Infrastructure.Persistence;
using GEE_Calculator.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEE_Calculator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GeeCalculatorDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
        services.AddScoped<IEmissionCalculationRepository, EmissionCalculationRepository>();

        return services;
    }
}
