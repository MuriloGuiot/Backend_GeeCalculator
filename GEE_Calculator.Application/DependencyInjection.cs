using GEE_Calculator.Application.Auth;
using GEE_Calculator.Application.ActivityEntries;
using GEE_Calculator.Application.Calculations;
using GEE_Calculator.Application.Companies;
using GEE_Calculator.Application.EmissionFactors;
using GEE_Calculator.Application.Inventories;
using GEE_Calculator.Application.Reports;
using GEE_Calculator.Application.Surveys;
using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.ActivityEntries;
using GEE_Calculator.Domain.Calculations;
using GEE_Calculator.Domain.Companies;
using GEE_Calculator.Domain.EmissionFactors;
using GEE_Calculator.Domain.Inventories;
using GEE_Calculator.Domain.Reports;
using GEE_Calculator.Domain.Surveys;
using Microsoft.Extensions.DependencyInjection;

namespace GEE_Calculator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IActivityEntryService, ActivityEntryService>();
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        services.AddScoped<IEmissionCalculationService, EmissionCalculationService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IEmissionFactorCatalogService, EmissionFactorCatalogService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISurveyService, SurveyService>();

        return services;
    }
}
