using GEE_Calculator.Application.Calculations;
using GEE_Calculator.Domain.Calculations;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Infrastructure.Auth;

namespace GEE_Calculator.Tests;

public sealed class EmissionCalculationServiceTests
{
    private static readonly EmissionCalculationService Service = new(calculationRepository: null!, currentTenantAccessor: null!);

    [Fact]
    public void Preview_ShouldCalculateEmissionTotals()
    {
        var request = new CalculateEmissionPreviewRequest(
            Scope: EmissionScope.Scope2,
            Category: "energia_eletrica_sin",
            ActivityValue: 1500m,
            ActivityUnit: "kWh",
            EmissionFactorKgCo2e: 0.0385m,
            Gwp: 1m);

        var response = Service.Preview(request);

        Assert.Equal(57.75m, response.TotalKgCo2e);
        Assert.Equal(0.05775m, response.TotalTCo2e);
        Assert.Equal(1m, response.CarbonCreditsRequired);
    }

    [Fact]
    public void Preview_ShouldRejectNegativeActivityValue()
    {
        var request = new CalculateEmissionPreviewRequest(
            Scope: EmissionScope.Scope1,
            Category: "diesel_rodoviario",
            ActivityValue: -1m,
            ActivityUnit: "L",
            EmissionFactorKgCo2e: 2.68m,
            Gwp: 1m);

        Assert.Throws<ArgumentOutOfRangeException>(() => Service.Preview(request));
    }

    [Fact]
    public void ComputeSha256_ShouldBeDeterministic()
    {
        var firstHash = ApiKeyValidator.ComputeSha256("gee_dev_local_2026");
        var secondHash = ApiKeyValidator.ComputeSha256("gee_dev_local_2026");

        Assert.Equal(firstHash, secondHash);
        Assert.NotEmpty(firstHash);
    }
}
