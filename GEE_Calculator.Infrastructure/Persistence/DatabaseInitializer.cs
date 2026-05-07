using GEE_Calculator.Infrastructure.Auth;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence;

public sealed class DatabaseInitializer(GeeCalculatorDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("PendingModelChangesWarning", StringComparison.Ordinal))
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await SeedCatalogsAsync(cancellationToken);
    }

    private async Task SeedCatalogsAsync(CancellationToken cancellationToken)
    {
        await EnsureActivityUnitAsync(SeedIds.UnitKwh, "kWh", "Kilowatt-hour", "energy", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitLiter, "L", "Liter", "volume", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitKm, "km", "Kilometer", "distance", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitCubicMeter, "m3", "Cubic meter", "volume", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitKg, "kg", "Kilogram", "mass", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitMetricTon, "t", "Metric ton", "mass", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitTonKilometer, "t.km", "Ton-kilometer", "freight_transport", cancellationToken);

        await EnsureEmissionCategoryAsync(SeedIds.CategoryElectricity, EmissionScope.Scope2, "energia_eletrica_sin", "Energia eletrica - SIN", "Purchased electricity using the Brazilian SIN location-based factor.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryDiesel, EmissionScope.Scope1, "diesel_rodoviario", "Diesel rodoviario", "Mobile combustion from road diesel consumed by owned or controlled fleet.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryAirTravel, EmissionScope.Scope3, "viagem_aerea", "Viagem aerea", "Business air travel estimated by passenger distance.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryGasoline, EmissionScope.Scope1, "gasolina_automotiva", "Gasolina automotiva", "Mobile combustion from gasoline consumed by owned or controlled fleet.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryEthanol, EmissionScope.Scope1, "etanol_hidratado", "Etanol hidratado", "Mobile combustion from hydrated ethanol consumed by owned or controlled fleet.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryNaturalGas, EmissionScope.Scope1, "gas_natural_estacionario", "Gas natural estacionario", "Stationary combustion from natural gas.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryGlp, EmissionScope.Scope1, "glp_estacionario", "GLP estacionario", "Stationary combustion from liquefied petroleum gas.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryRefrigerant, EmissionScope.Scope1, "refrigerante_hfc", "Recarga de refrigerantes HFC", "Fugitive emissions from refrigerant recharge.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryMarketElectricity, EmissionScope.Scope2, "energia_eletrica_mercado_livre", "Energia eletrica - mercado livre", "Purchased electricity using a market-based contractual factor.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryPurchasedSteam, EmissionScope.Scope2, "vapor_adquirido", "Vapor adquirido", "Purchased steam, heat or cooling.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryFreightRoad, EmissionScope.Scope3, "transporte_cargas_rodoviario", "Transporte de cargas rodoviario", "Upstream or downstream third-party road freight by ton-kilometer.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryEmployeeCommute, EmissionScope.Scope3, "deslocamento_colaboradores", "Deslocamento de colaboradores", "Employee commuting by distance.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryWasteLandfill, EmissionScope.Scope3, "residuos_aterro", "Residuos enviados a aterro", "Waste generated in operations and sent to landfill.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryPurchasedGoods, EmissionScope.Scope3, "bens_servicos_adquiridos", "Bens e servicos adquiridos", "Purchased goods and services by spend or mass proxy.", cancellationToken);

        if (!await dbContext.GreenhouseGases.AnyAsync(cancellationToken))
        {
            dbContext.GreenhouseGases.Add(new GreenhouseGas
            {
                Id = SeedIds.GasCo2e,
                Code = "CO2E",
                Name = "Carbon Dioxide Equivalent",
                DefaultGwp = 1m,
                GwpSource = "Default CO2e aggregation",
                VersionYear = 2026
            });
        }

        if (!await dbContext.EmissionFactorSources.AnyAsync(cancellationToken))
        {
            dbContext.EmissionFactorSources.Add(new EmissionFactorSource
            {
                Id = SeedIds.SourceDevSeed,
                Code = "agrocarbonbr_dev_seed",
                Name = "AgrocarbonBR Development Seed",
                Publisher = "AgrocarbonBR",
                SourceUrl = "https://example.local/dev-seed"
            });
        }

        if (!await dbContext.EmissionFactorSets.AnyAsync(cancellationToken))
        {
            dbContext.EmissionFactorSets.Add(new EmissionFactorSet
            {
                Id = SeedIds.FactorSet2026,
                SourceId = SeedIds.SourceDevSeed,
                Code = "dev_seed_2026",
                Name = "Development seed factors 2026",
                VersionLabel = "v2026",
                VersionYear = 2026
            });
        }

        await EnsureEmissionFactorAsync(SeedIds.FactorElectricity, SeedIds.CategoryElectricity, SeedIds.UnitKwh, 0.0385m, "Formula: kWh x kgCO2e/kWh. Development seed for SIN electricity.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorDiesel, SeedIds.CategoryDiesel, SeedIds.UnitLiter, 2.68m, "Formula: liters x kgCO2e/L. Development seed for road diesel.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorAirTravel, SeedIds.CategoryAirTravel, SeedIds.UnitKm, 0.15m, "Formula: passenger-km x kgCO2e/km. Development seed for business air travel.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorGasoline, SeedIds.CategoryGasoline, SeedIds.UnitLiter, 2.31m, "Formula: liters x kgCO2e/L. Development seed for automotive gasoline.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorEthanol, SeedIds.CategoryEthanol, SeedIds.UnitLiter, 1.51m, "Formula: liters x kgCO2e/L. Development seed for hydrated ethanol.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorNaturalGas, SeedIds.CategoryNaturalGas, SeedIds.UnitCubicMeter, 1.90m, "Formula: m3 x kgCO2e/m3. Development seed for stationary natural gas.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorGlp, SeedIds.CategoryGlp, SeedIds.UnitKg, 3.00m, "Formula: kg x kgCO2e/kg. Development seed for GLP.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorRefrigerant, SeedIds.CategoryRefrigerant, SeedIds.UnitKg, 1430m, "Formula: kg leaked/recharged x GWP. Development seed using HFC proxy.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorMarketElectricity, SeedIds.CategoryMarketElectricity, SeedIds.UnitKwh, 0.0200m, "Formula: kWh x contractual kgCO2e/kWh. Development seed for market-based electricity.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorPurchasedSteam, SeedIds.CategoryPurchasedSteam, SeedIds.UnitKwh, 0.0700m, "Formula: kWh thermal equivalent x kgCO2e/kWh. Development seed for purchased steam/heat.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorFreightRoad, SeedIds.CategoryFreightRoad, SeedIds.UnitTonKilometer, 0.085m, "Formula: ton-km x kgCO2e/t.km. Development seed for third-party road freight.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorEmployeeCommute, SeedIds.CategoryEmployeeCommute, SeedIds.UnitKm, 0.12m, "Formula: km x kgCO2e/km. Development seed for employee commuting.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorWasteLandfill, SeedIds.CategoryWasteLandfill, SeedIds.UnitMetricTon, 580m, "Formula: metric tons x kgCO2e/t. Development seed for landfill waste.", cancellationToken);
        await EnsureEmissionFactorAsync(SeedIds.FactorPurchasedGoods, SeedIds.CategoryPurchasedGoods, SeedIds.UnitMetricTon, 400m, "Formula: metric tons x kgCO2e/t proxy. Development seed for purchased goods/services.", cancellationToken);

        if (!await dbContext.Tenants.AnyAsync(item => item.Id == SeedIds.DevTenant, cancellationToken))
        {
            dbContext.Tenants.Add(new Tenant
            {
                Id = SeedIds.DevTenant,
                ExternalTenantId = SeedIds.DevTenant.ToString(),
                Name = "Development Tenant"
            });
        }

        if (!await dbContext.ApiClients.AnyAsync(item => item.Id == SeedIds.DevApiClient, cancellationToken))
        {
            dbContext.ApiClients.Add(new ApiClient
            {
                Id = SeedIds.DevApiClient,
                TenantId = SeedIds.DevTenant,
                Name = "Development Local Client",
                KeyPrefix = "gee_dev_",
                KeyHash = ApiKeyValidator.ComputeSha256(SeedValues.DevelopmentApiKey)
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureActivityUnitAsync(
        Guid id,
        string code,
        string name,
        string dimension,
        CancellationToken cancellationToken)
    {
        if (await dbContext.ActivityUnits.AnyAsync(item => item.Code == code, cancellationToken))
        {
            return;
        }

        dbContext.ActivityUnits.Add(new ActivityUnit { Id = id, Code = code, Name = name, Dimension = dimension });
    }

    private async Task EnsureEmissionCategoryAsync(
        Guid id,
        EmissionScope scope,
        string code,
        string name,
        string description,
        CancellationToken cancellationToken)
    {
        if (await dbContext.EmissionCategories.AnyAsync(item => item.Code == code, cancellationToken))
        {
            return;
        }

        dbContext.EmissionCategories.Add(new EmissionCategory
        {
            Id = id,
            Scope = scope,
            Code = code,
            Name = name,
            Description = description
        });
    }

    private async Task EnsureEmissionFactorAsync(
        Guid id,
        Guid categoryId,
        Guid activityUnitId,
        decimal factorKgCo2ePerUnit,
        string calculationNotes,
        CancellationToken cancellationToken)
    {
        if (await dbContext.EmissionFactors.AnyAsync(item => item.Id == id, cancellationToken))
        {
            return;
        }

        dbContext.EmissionFactors.Add(new EmissionFactor
        {
            Id = id,
            FactorSetId = SeedIds.FactorSet2026,
            CategoryId = categoryId,
            ActivityUnitId = activityUnitId,
            GasId = SeedIds.GasCo2e,
            Gwp = 1m,
            FactorKgCo2ePerUnit = factorKgCo2ePerUnit,
            CalculationNotes = calculationNotes
        });
    }

    public static class SeedIds
    {
        public static readonly Guid UnitKwh = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid UnitLiter = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid UnitKm = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid UnitCubicMeter = Guid.Parse("10000000-0000-0000-0000-000000000004");
        public static readonly Guid UnitKg = Guid.Parse("10000000-0000-0000-0000-000000000005");
        public static readonly Guid UnitMetricTon = Guid.Parse("10000000-0000-0000-0000-000000000006");
        public static readonly Guid UnitTonKilometer = Guid.Parse("10000000-0000-0000-0000-000000000007");

        public static readonly Guid CategoryElectricity = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid CategoryDiesel = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid CategoryAirTravel = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public static readonly Guid CategoryGasoline = Guid.Parse("20000000-0000-0000-0000-000000000004");
        public static readonly Guid CategoryEthanol = Guid.Parse("20000000-0000-0000-0000-000000000005");
        public static readonly Guid CategoryNaturalGas = Guid.Parse("20000000-0000-0000-0000-000000000006");
        public static readonly Guid CategoryGlp = Guid.Parse("20000000-0000-0000-0000-000000000007");
        public static readonly Guid CategoryRefrigerant = Guid.Parse("20000000-0000-0000-0000-000000000008");
        public static readonly Guid CategoryMarketElectricity = Guid.Parse("20000000-0000-0000-0000-000000000009");
        public static readonly Guid CategoryPurchasedSteam = Guid.Parse("20000000-0000-0000-0000-000000000010");
        public static readonly Guid CategoryFreightRoad = Guid.Parse("20000000-0000-0000-0000-000000000011");
        public static readonly Guid CategoryEmployeeCommute = Guid.Parse("20000000-0000-0000-0000-000000000012");
        public static readonly Guid CategoryWasteLandfill = Guid.Parse("20000000-0000-0000-0000-000000000013");
        public static readonly Guid CategoryPurchasedGoods = Guid.Parse("20000000-0000-0000-0000-000000000014");

        public static readonly Guid GasCo2e = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid SourceDevSeed = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public static readonly Guid FactorSet2026 = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public static readonly Guid DevTenant = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid DevApiClient = Guid.Parse("70000000-0000-0000-0000-000000000001");

        public static readonly Guid FactorElectricity = Guid.Parse("60000000-0000-0000-0000-000000000001");
        public static readonly Guid FactorDiesel = Guid.Parse("60000000-0000-0000-0000-000000000002");
        public static readonly Guid FactorAirTravel = Guid.Parse("60000000-0000-0000-0000-000000000003");
        public static readonly Guid FactorGasoline = Guid.Parse("60000000-0000-0000-0000-000000000004");
        public static readonly Guid FactorEthanol = Guid.Parse("60000000-0000-0000-0000-000000000005");
        public static readonly Guid FactorNaturalGas = Guid.Parse("60000000-0000-0000-0000-000000000006");
        public static readonly Guid FactorGlp = Guid.Parse("60000000-0000-0000-0000-000000000007");
        public static readonly Guid FactorRefrigerant = Guid.Parse("60000000-0000-0000-0000-000000000008");
        public static readonly Guid FactorMarketElectricity = Guid.Parse("60000000-0000-0000-0000-000000000009");
        public static readonly Guid FactorPurchasedSteam = Guid.Parse("60000000-0000-0000-0000-000000000010");
        public static readonly Guid FactorFreightRoad = Guid.Parse("60000000-0000-0000-0000-000000000011");
        public static readonly Guid FactorEmployeeCommute = Guid.Parse("60000000-0000-0000-0000-000000000012");
        public static readonly Guid FactorWasteLandfill = Guid.Parse("60000000-0000-0000-0000-000000000013");
        public static readonly Guid FactorPurchasedGoods = Guid.Parse("60000000-0000-0000-0000-000000000014");
    }

    public static class SeedValues
    {
        public const string DevelopmentApiKey = "gee_dev_local_2026";
    }
}
