using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Calculations;

public sealed record CalculateEmissionPreviewResponse(
    EmissionScope Scope,
    string Category,
    decimal ActivityValue,
    string ActivityUnit,
    decimal EmissionFactorKgCo2e,
    decimal Gwp,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal CarbonCreditsRequired);
