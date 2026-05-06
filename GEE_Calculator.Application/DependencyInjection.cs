using GEE_Calculator.Application.Calculations;
using GEE_Calculator.Domain.Calculations;
using Microsoft.Extensions.DependencyInjection;

namespace GEE_Calculator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmissionCalculationService, EmissionCalculationService>();

        return services;
    }
}
