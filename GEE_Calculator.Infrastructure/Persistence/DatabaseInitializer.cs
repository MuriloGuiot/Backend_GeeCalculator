using GEE_Calculator.Infrastructure.Auth;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence;

public sealed class DatabaseInitializer(GeeCalculatorDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
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
        await EnsureActivityUnitAsync(SeedIds.UnitMwh, "MWh", "Megawatt-hour", "energy", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitKgCo2e, "kgCO2e", "Kilogram of carbon dioxide equivalent", "emission", cancellationToken);
        await EnsureActivityUnitAsync(SeedIds.UnitTCo2e, "tCO2e", "Metric ton of carbon dioxide equivalent", "emission", cancellationToken);

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
        await EnsureEmissionCategoryAsync(SeedIds.CategoryLandUseChangeCh4, EmissionScope.Scope1, "mudanca_uso_solo_ch4", "Mudanca no uso do solo - CH4", "Scope 1 land-use change reported as methane mass converted by GWP.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryRefrigerantR404A, EmissionScope.Scope1, "refrigerante_r404a", "Refrigerante R-404A", "Fugitive RAC emissions for refrigerant R-404A.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryRefrigerantR407C, EmissionScope.Scope1, "refrigerante_r407c", "Refrigerante R-407C", "Fugitive RAC emissions for refrigerant R-407C.", cancellationToken);
        await EnsureEmissionCategoryAsync(SeedIds.CategoryRefrigerantHfc134A, EmissionScope.Scope1, "refrigerante_hfc134a", "Refrigerante HFC-134a", "Fugitive RAC emissions for refrigerant HFC-134a.", cancellationToken);

        await EnsureGreenhouseGasAsync(SeedIds.GasCo2e, "CO2E", "Carbon Dioxide Equivalent", 1m, "Default CO2e aggregation", 2026, cancellationToken);
        await EnsureGreenhouseGasAsync(SeedIds.GasCo2, "CO2", "Carbon Dioxide", 1m, "GHG Protocol/IPCC default seed", 2026, cancellationToken);
        await EnsureGreenhouseGasAsync(SeedIds.GasCh4, "CH4", "Methane", 28m, "GHG Protocol/IPCC default seed", 2026, cancellationToken);
        await EnsureGreenhouseGasAsync(SeedIds.GasN2O, "N2O", "Nitrous Oxide", 265m, "GHG Protocol/IPCC default seed", 2026, cancellationToken);
        await EnsureGreenhouseGasAsync(SeedIds.GasHfc134A, "HFC-134a", "HFC-134a", 1300m, "PBGHG 2026 tool seed", 2026, cancellationToken);
        await EnsureGreenhouseGasAsync(SeedIds.GasR404A, "R-404A", "R-404A", 3942.8m, "PBGHG 2026 tool seed", 2026, cancellationToken);
        await EnsureGreenhouseGasAsync(SeedIds.GasR407C, "R-407C", "R-407C", 1624.21m, "PBGHG 2026 tool seed", 2026, cancellationToken);

        if (!await dbContext.EmissionFactorSources.AnyAsync(cancellationToken))
        {
            dbContext.EmissionFactorSources.Add(new EmissionFactorSource
            {
                Id = SeedIds.SourceDevSeed,
                Code = "agrocarbonbr_dev_seed",
                Name = "AgrocarbonBR Development Seed",
                Publisher = "AgrocarbonBR",
                SourceUrl = "local://agrocarbonbr/dev-seed"
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
        await EnsureEmissionFactorAsync(SeedIds.FactorElectricityMwh, SeedIds.CategoryElectricity, SeedIds.UnitMwh, 38.5m, "Formula: MWh x kgCO2e/MWh. Development seed for SIN electricity aligned with GHG workbook inputs.", cancellationToken);
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
        await EnsureEmissionFactorAsync(SeedIds.FactorLandUseCh4, SeedIds.CategoryLandUseChangeCh4, SeedIds.UnitMetricTon, 28000m, "Formula: metric tons CH4 x 28 GWP x 1000 kg/t. Acceptance seed for land-use change CH4 reports.", cancellationToken, SeedIds.GasCh4, 1000m, 28m);
        await EnsureEmissionFactorAsync(SeedIds.FactorRefrigerantR404A, SeedIds.CategoryRefrigerantR404A, SeedIds.UnitKg, 3942.8m, "Formula: kg leaked/recharged x R-404A GWP.", cancellationToken, SeedIds.GasR404A, 1m, 3942.8m);
        await EnsureEmissionFactorAsync(SeedIds.FactorRefrigerantR407C, SeedIds.CategoryRefrigerantR407C, SeedIds.UnitKg, 1624.21m, "Formula: kg leaked/recharged x R-407C GWP.", cancellationToken, SeedIds.GasR407C, 1m, 1624.21m);
        await EnsureEmissionFactorAsync(SeedIds.FactorRefrigerantHfc134A, SeedIds.CategoryRefrigerantHfc134A, SeedIds.UnitKg, 1300m, "Formula: kg leaked/recharged x HFC-134a GWP.", cancellationToken, SeedIds.GasHfc134A, 1m, 1300m);

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

        await EnsureDefaultSurveyAsync(cancellationToken);

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

    private async Task EnsureGreenhouseGasAsync(
        Guid id,
        string code,
        string name,
        decimal defaultGwp,
        string gwpSource,
        int versionYear,
        CancellationToken cancellationToken)
    {
        if (await dbContext.GreenhouseGases.AnyAsync(
            item => item.Code == code && item.VersionYear == versionYear,
            cancellationToken))
        {
            return;
        }

        dbContext.GreenhouseGases.Add(new GreenhouseGas
        {
            Id = id,
            Code = code,
            Name = name,
            DefaultGwp = defaultGwp,
            GwpSource = gwpSource,
            VersionYear = versionYear
        });
    }

    private async Task EnsureEmissionFactorAsync(
        Guid id,
        Guid categoryId,
        Guid activityUnitId,
        decimal factorKgCo2ePerUnit,
        string calculationNotes,
        CancellationToken cancellationToken,
        Guid? gasId = null,
        decimal? factorKgPerUnit = null,
        decimal? gwp = null)
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
            GasId = gasId ?? SeedIds.GasCo2e,
            FactorKgPerUnit = factorKgPerUnit,
            Gwp = gwp ?? 1m,
            FactorKgCo2ePerUnit = factorKgCo2ePerUnit,
            CalculationNotes = calculationNotes
        });
    }

    private async Task EnsureDefaultSurveyAsync(CancellationToken cancellationToken)
    {
        await EnsureSurveyTemplateAsync(
            SeedIds.SurveyTemplateMonthOne,
            "gogreen_month_1_v1",
            "GoGreen GHG Protocol - Mes 1",
            "month-1-v1",
            SeedIds.FactorSet2026,
            cancellationToken);

        await EnsureSurveySectionAsync(SeedIds.SurveySectionMobile, "escopo_1_combustao_movel", "Escopo 1 - Combustao movel", 10, cancellationToken);
        await EnsureSurveySectionAsync(SeedIds.SurveySectionFugitives, "escopo_1_fugitivas", "Escopo 1 - Emissoes fugitivas", 20, cancellationToken);
        await EnsureSurveySectionAsync(SeedIds.SurveySectionLandUse, "escopo_1_mudanca_uso_solo", "Escopo 1 - Mudanca no uso do solo", 30, cancellationToken);
        await EnsureSurveySectionAsync(SeedIds.SurveySectionElectricity, "escopo_2_energia_eletrica", "Escopo 2 - Energia eletrica", 40, cancellationToken);
        await EnsureSurveySectionAsync(SeedIds.SurveySectionScope3, "escopo_3_triagem", "Escopo 3 - Triagem de categorias", 50, cancellationToken);

        await EnsureSurveyQuestionAsync(
            SeedIds.SurveyQuestionHasFleet,
            SeedIds.SurveySectionMobile,
            "possui_frota_controlada",
            "A organizacao possui frota propria ou controlada?",
            "boolean",
            true,
            10,
            "{}",
            "{\"surveyOnly\":true}",
            cancellationToken);
        await EnsureYesNoOptionsAsync(SeedIds.SurveyQuestionHasFleet, cancellationToken);

        await EnsureSurveyQuestionAsync(
            SeedIds.SurveyQuestionGasolineLiters,
            SeedIds.SurveySectionMobile,
            "gasolina_litros",
            "Qual foi o consumo de gasolina automotiva da frota controlada?",
            "decimal",
            false,
            20,
            "{\"when\":{\"questionCode\":\"possui_frota_controlada\",\"equals\":\"yes\"}}",
            "{\"entry\":{\"categoryCode\":\"gasolina_automotiva\",\"activityUnitCode\":\"L\",\"calculationMethod\":\"factor\"}}",
            cancellationToken);

        await EnsureSurveyQuestionAsync(
            SeedIds.SurveyQuestionR404AKg,
            SeedIds.SurveySectionFugitives,
            "r404a_kg",
            "Qual massa de R-404A foi recarregada ou perdida em equipamentos RAC?",
            "decimal",
            false,
            10,
            "{}",
            "{\"entry\":{\"categoryCode\":\"refrigerante_r404a\",\"activityUnitCode\":\"kg\",\"calculationMethod\":\"factor\"}}",
            cancellationToken);

        await EnsureSurveyQuestionAsync(
            SeedIds.SurveyQuestionLandUseCh4Tco2e,
            SeedIds.SurveySectionLandUse,
            "mudanca_uso_solo_ch4_tco2e",
            "Qual o total reportado de CH4 de mudanca no uso do solo em tCO2e?",
            "decimal",
            false,
            10,
            "{}",
            "{\"entry\":{\"categoryCode\":\"mudanca_uso_solo_ch4\",\"activityUnitCode\":\"tCO2e\",\"calculationMethod\":\"reported_total\"}}",
            cancellationToken);

        await EnsureSurveyQuestionAsync(
            SeedIds.SurveyQuestionLandUseRemovalTco2,
            SeedIds.SurveySectionLandUse,
            "mudanca_uso_solo_remocao_tco2",
            "Qual o total de remocoes biogenicas de mudanca no uso do solo em tCO2?",
            "decimal",
            false,
            20,
            "{}",
            "{\"metadata\":{\"targetQuestionCode\":\"mudanca_uso_solo_ch4_tco2e\",\"field\":\"biogenicRemovalTCo2\"}}",
            cancellationToken);

        await EnsureSurveyQuestionAsync(
            SeedIds.SurveyQuestionElectricityMwh,
            SeedIds.SurveySectionElectricity,
            "energia_eletrica_sin_mwh",
            "Qual foi a eletricidade comprada do SIN em MWh?",
            "decimal",
            false,
            10,
            "{}",
            "{\"entry\":{\"categoryCode\":\"energia_eletrica_sin\",\"activityUnitCode\":\"MWh\",\"calculationMethod\":\"factor\"}}",
            cancellationToken);

        await EnsureSurveyQuestionAsync(
            SeedIds.SurveyQuestionScope3Categories,
            SeedIds.SurveySectionScope3,
            "categorias_escopo_3_relevantes",
            "Quais categorias de Escopo 3 sao relevantes para a organizacao?",
            "multiselect",
            false,
            10,
            "{}",
            "{\"surveyOnly\":true,\"next\":\"scope_3_prioritization\"}",
            cancellationToken);
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionScope3Cat1, SeedIds.SurveyQuestionScope3Categories, "cat_1", "Categoria 1 - Bens e servicos comprados", "cat_1", 10, cancellationToken);
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionScope3Cat3, SeedIds.SurveyQuestionScope3Categories, "cat_3", "Categoria 3 - Energia upstream", "cat_3", 20, cancellationToken);
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionScope3Cat4, SeedIds.SurveyQuestionScope3Categories, "cat_4", "Categoria 4 - Transporte e distribuicao upstream", "cat_4", 30, cancellationToken);
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionScope3Cat5, SeedIds.SurveyQuestionScope3Categories, "cat_5", "Categoria 5 - Residuos gerados nas operacoes", "cat_5", 40, cancellationToken);
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionScope3Cat6, SeedIds.SurveyQuestionScope3Categories, "cat_6", "Categoria 6 - Viagens a negocios", "cat_6", 50, cancellationToken);
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionScope3Cat7, SeedIds.SurveyQuestionScope3Categories, "cat_7", "Categoria 7 - Deslocamento casa-trabalho", "cat_7", 60, cancellationToken);
    }

    private async Task EnsureSurveyTemplateAsync(Guid id, string code, string name, string versionLabel, Guid factorSetId, CancellationToken cancellationToken)
    {
        if (await dbContext.SurveyTemplates.AnyAsync(item => item.Code == code, cancellationToken))
        {
            return;
        }

        dbContext.SurveyTemplates.Add(new SurveyTemplate
        {
            Id = id,
            Code = code,
            Name = name,
            VersionLabel = versionLabel,
            FactorSetId = factorSetId,
            IsActive = true
        });
    }

    private async Task EnsureSurveySectionAsync(Guid id, string code, string title, int sortOrder, CancellationToken cancellationToken)
    {
        if (await dbContext.SurveySections.AnyAsync(item => item.TemplateId == SeedIds.SurveyTemplateMonthOne && item.Code == code, cancellationToken))
        {
            return;
        }

        dbContext.SurveySections.Add(new SurveySection
        {
            Id = id,
            TemplateId = SeedIds.SurveyTemplateMonthOne,
            Code = code,
            Title = title,
            SortOrder = sortOrder
        });
    }

    private async Task EnsureSurveyQuestionAsync(
        Guid id,
        Guid sectionId,
        string code,
        string prompt,
        string answerType,
        bool isRequired,
        int sortOrder,
        string visibilityRuleJson,
        string mappingJson,
        CancellationToken cancellationToken)
    {
        if (await dbContext.SurveyQuestions.AnyAsync(item => item.SectionId == sectionId && item.Code == code, cancellationToken))
        {
            return;
        }

        dbContext.SurveyQuestions.Add(new SurveyQuestion
        {
            Id = id,
            SectionId = sectionId,
            Code = code,
            Prompt = prompt,
            AnswerType = answerType,
            IsRequired = isRequired,
            SortOrder = sortOrder,
            VisibilityRuleJson = visibilityRuleJson,
            MappingJson = mappingJson
        });
    }

    private async Task EnsureYesNoOptionsAsync(Guid questionId, CancellationToken cancellationToken)
    {
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionHasFleetYes, questionId, "yes", "Sim", "yes", 10, cancellationToken);
        await EnsureSurveyOptionAsync(SeedIds.SurveyOptionHasFleetNo, questionId, "no", "Nao", "no", 20, cancellationToken);
    }

    private async Task EnsureSurveyOptionAsync(
        Guid id,
        Guid questionId,
        string code,
        string label,
        string value,
        int sortOrder,
        CancellationToken cancellationToken)
    {
        if (await dbContext.SurveyOptions.AnyAsync(item => item.QuestionId == questionId && item.Code == code, cancellationToken))
        {
            return;
        }

        dbContext.SurveyOptions.Add(new SurveyOption
        {
            Id = id,
            QuestionId = questionId,
            Code = code,
            Label = label,
            Value = value,
            SortOrder = sortOrder
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
        public static readonly Guid UnitMwh = Guid.Parse("10000000-0000-0000-0000-000000000008");
        public static readonly Guid UnitKgCo2e = Guid.Parse("10000000-0000-0000-0000-000000000009");
        public static readonly Guid UnitTCo2e = Guid.Parse("10000000-0000-0000-0000-000000000010");

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
        public static readonly Guid CategoryLandUseChangeCh4 = Guid.Parse("20000000-0000-0000-0000-000000000015");
        public static readonly Guid CategoryRefrigerantR404A = Guid.Parse("20000000-0000-0000-0000-000000000016");
        public static readonly Guid CategoryRefrigerantR407C = Guid.Parse("20000000-0000-0000-0000-000000000017");
        public static readonly Guid CategoryRefrigerantHfc134A = Guid.Parse("20000000-0000-0000-0000-000000000018");

        public static readonly Guid GasCo2e = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid GasCo2 = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public static readonly Guid GasCh4 = Guid.Parse("30000000-0000-0000-0000-000000000003");
        public static readonly Guid GasN2O = Guid.Parse("30000000-0000-0000-0000-000000000004");
        public static readonly Guid GasHfc134A = Guid.Parse("30000000-0000-0000-0000-000000000005");
        public static readonly Guid GasR404A = Guid.Parse("30000000-0000-0000-0000-000000000006");
        public static readonly Guid GasR407C = Guid.Parse("30000000-0000-0000-0000-000000000007");
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
        public static readonly Guid FactorElectricityMwh = Guid.Parse("60000000-0000-0000-0000-000000000015");
        public static readonly Guid FactorLandUseCh4 = Guid.Parse("60000000-0000-0000-0000-000000000016");
        public static readonly Guid FactorRefrigerantR404A = Guid.Parse("60000000-0000-0000-0000-000000000017");
        public static readonly Guid FactorRefrigerantR407C = Guid.Parse("60000000-0000-0000-0000-000000000018");
        public static readonly Guid FactorRefrigerantHfc134A = Guid.Parse("60000000-0000-0000-0000-000000000019");

        public static readonly Guid SurveyTemplateMonthOne = Guid.Parse("80000000-0000-0000-0000-000000000001");
        public static readonly Guid SurveySectionMobile = Guid.Parse("81000000-0000-0000-0000-000000000001");
        public static readonly Guid SurveySectionFugitives = Guid.Parse("81000000-0000-0000-0000-000000000002");
        public static readonly Guid SurveySectionLandUse = Guid.Parse("81000000-0000-0000-0000-000000000003");
        public static readonly Guid SurveySectionElectricity = Guid.Parse("81000000-0000-0000-0000-000000000004");
        public static readonly Guid SurveySectionScope3 = Guid.Parse("81000000-0000-0000-0000-000000000005");
        public static readonly Guid SurveyQuestionHasFleet = Guid.Parse("82000000-0000-0000-0000-000000000001");
        public static readonly Guid SurveyQuestionGasolineLiters = Guid.Parse("82000000-0000-0000-0000-000000000002");
        public static readonly Guid SurveyQuestionR404AKg = Guid.Parse("82000000-0000-0000-0000-000000000003");
        public static readonly Guid SurveyQuestionLandUseCh4Tco2e = Guid.Parse("82000000-0000-0000-0000-000000000004");
        public static readonly Guid SurveyQuestionLandUseRemovalTco2 = Guid.Parse("82000000-0000-0000-0000-000000000005");
        public static readonly Guid SurveyQuestionElectricityMwh = Guid.Parse("82000000-0000-0000-0000-000000000006");
        public static readonly Guid SurveyQuestionScope3Categories = Guid.Parse("82000000-0000-0000-0000-000000000007");
        public static readonly Guid SurveyOptionHasFleetYes = Guid.Parse("83000000-0000-0000-0000-000000000101");
        public static readonly Guid SurveyOptionHasFleetNo = Guid.Parse("83000000-0000-0000-0000-000000000102");
        public static readonly Guid SurveyOptionScope3Cat1 = Guid.Parse("83000000-0000-0000-0000-000000000001");
        public static readonly Guid SurveyOptionScope3Cat3 = Guid.Parse("83000000-0000-0000-0000-000000000003");
        public static readonly Guid SurveyOptionScope3Cat4 = Guid.Parse("83000000-0000-0000-0000-000000000004");
        public static readonly Guid SurveyOptionScope3Cat5 = Guid.Parse("83000000-0000-0000-0000-000000000005");
        public static readonly Guid SurveyOptionScope3Cat6 = Guid.Parse("83000000-0000-0000-0000-000000000006");
        public static readonly Guid SurveyOptionScope3Cat7 = Guid.Parse("83000000-0000-0000-0000-000000000007");
    }

    public static class SeedValues
    {
        public const string DevelopmentApiKey = "gee_dev_local_2026";
    }
}
