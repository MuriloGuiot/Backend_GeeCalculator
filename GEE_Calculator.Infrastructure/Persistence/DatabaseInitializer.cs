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
        if (!await dbContext.ActivityUnits.AnyAsync(cancellationToken))
        {
            dbContext.ActivityUnits.AddRange(
                new ActivityUnit { Id = SeedIds.UnitKwh, Code = "kWh", Name = "Kilowatt-hour", Dimension = "energy" },
                new ActivityUnit { Id = SeedIds.UnitLiter, Code = "L", Name = "Liter", Dimension = "volume" },
                new ActivityUnit { Id = SeedIds.UnitKm, Code = "km", Name = "Kilometer", Dimension = "distance" });
        }

        if (!await dbContext.EmissionCategories.AnyAsync(cancellationToken))
        {
            dbContext.EmissionCategories.AddRange(
                new EmissionCategory { Id = SeedIds.CategoryElectricity, Scope = EmissionScope.Scope2, Code = "energia_eletrica_sin", Name = "Energia eletrica - SIN" },
                new EmissionCategory { Id = SeedIds.CategoryDiesel, Scope = EmissionScope.Scope1, Code = "diesel_rodoviario", Name = "Diesel rodoviario" },
                new EmissionCategory { Id = SeedIds.CategoryAirTravel, Scope = EmissionScope.Scope3, Code = "viagem_aerea", Name = "Viagem aerea" });
        }

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

        if (!await dbContext.EmissionFactors.AnyAsync(cancellationToken))
        {
            dbContext.EmissionFactors.AddRange(
                new EmissionFactor
                {
                    Id = SeedIds.FactorElectricity,
                    FactorSetId = SeedIds.FactorSet2026,
                    CategoryId = SeedIds.CategoryElectricity,
                    ActivityUnitId = SeedIds.UnitKwh,
                    GasId = SeedIds.GasCo2e,
                    Gwp = 1m,
                    FactorKgCo2ePerUnit = 0.0385m,
                    CalculationNotes = "Seed factor for SIN electricity."
                },
                new EmissionFactor
                {
                    Id = SeedIds.FactorDiesel,
                    FactorSetId = SeedIds.FactorSet2026,
                    CategoryId = SeedIds.CategoryDiesel,
                    ActivityUnitId = SeedIds.UnitLiter,
                    GasId = SeedIds.GasCo2e,
                    Gwp = 1m,
                    FactorKgCo2ePerUnit = 2.68m,
                    CalculationNotes = "Seed factor for road diesel."
                },
                new EmissionFactor
                {
                    Id = SeedIds.FactorAirTravel,
                    FactorSetId = SeedIds.FactorSet2026,
                    CategoryId = SeedIds.CategoryAirTravel,
                    ActivityUnitId = SeedIds.UnitKm,
                    GasId = SeedIds.GasCo2e,
                    Gwp = 1m,
                    FactorKgCo2ePerUnit = 0.15m,
                    CalculationNotes = "Seed factor for air travel per km."
                });
        }

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

    public static class SeedIds
    {
        public static readonly Guid UnitKwh = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid UnitLiter = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid UnitKm = Guid.Parse("10000000-0000-0000-0000-000000000003");

        public static readonly Guid CategoryElectricity = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid CategoryDiesel = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid CategoryAirTravel = Guid.Parse("20000000-0000-0000-0000-000000000003");

        public static readonly Guid GasCo2e = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid SourceDevSeed = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public static readonly Guid FactorSet2026 = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public static readonly Guid DevTenant = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid DevApiClient = Guid.Parse("70000000-0000-0000-0000-000000000001");

        public static readonly Guid FactorElectricity = Guid.Parse("60000000-0000-0000-0000-000000000001");
        public static readonly Guid FactorDiesel = Guid.Parse("60000000-0000-0000-0000-000000000002");
        public static readonly Guid FactorAirTravel = Guid.Parse("60000000-0000-0000-0000-000000000003");
    }

    public static class SeedValues
    {
        public const string DevelopmentApiKey = "gee_dev_local_2026";
    }
}
