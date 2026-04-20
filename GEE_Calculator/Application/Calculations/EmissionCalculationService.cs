namespace GEE_Calculator.Application.Calculations;

public sealed class EmissionCalculationService : IEmissionCalculationService
{
    public CalculateEmissionPreviewResponse Preview(CalculateEmissionPreviewRequest request)
    {
        if (request.ActivityValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.ActivityValue), "Activity value cannot be negative.");
        }

        if (request.EmissionFactorKgCo2e < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.EmissionFactorKgCo2e), "Emission factor cannot be negative.");
        }

        if (request.Gwp <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Gwp), "GWP must be greater than zero.");
        }

        var totalKgCo2e = request.ActivityValue * request.EmissionFactorKgCo2e * request.Gwp;
        var totalTCo2e = totalKgCo2e / 1000m;
        var carbonCreditsRequired = Math.Ceiling(totalTCo2e);

        return new CalculateEmissionPreviewResponse(
            request.Scope,
            request.Category,
            request.ActivityValue,
            request.ActivityUnit,
            request.EmissionFactorKgCo2e,
            request.Gwp,
            totalKgCo2e,
            totalTCo2e,
            carbonCreditsRequired);
    }
}
