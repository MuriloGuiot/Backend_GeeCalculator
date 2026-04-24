using GEE_Calculator.Application.Auth;
using GEE_Calculator.Application.Calculations;
using GEE_Calculator.Application.Tenancy;

namespace GEE_Calculator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenantAccessor, CurrentTenantAccessor>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IEmissionCalculationService, EmissionCalculationService>();

        return services;
    }
}
