using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.ActivityEntries;
using GEE_Calculator.Domain.Calculations;
using GEE_Calculator.Domain.Companies;
using GEE_Calculator.Domain.EmissionFactors;
using GEE_Calculator.Domain.Inventories;
using GEE_Calculator.Domain.Reports;
using GEE_Calculator.Domain.Surveys;
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
        services.AddScoped<IActivityEntryRepository, ActivityEntryRepository>();
        services.AddScoped<IEmissionCalculationRepository, EmissionCalculationRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IEmissionFactorCatalogRepository, EmissionFactorCatalogRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<ISurveyRepository, SurveyRepository>();

        return services;
    }
}
