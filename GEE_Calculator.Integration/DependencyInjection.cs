using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Integration.GoGreen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEE_Calculator.Integration;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoGreenIdentityOptions>(configuration.GetSection("GoGreen"));
        services.AddHttpClient<IGoGreenIdentityClient, GoGreenIdentityClient>();

        return services;
    }
}
