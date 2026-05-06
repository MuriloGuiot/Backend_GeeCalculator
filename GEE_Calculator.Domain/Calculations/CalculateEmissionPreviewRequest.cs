using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Calculations;

public sealed record CalculateEmissionPreviewRequest(
    EmissionScope Scope,
    string Category,
    decimal ActivityValue,
    string ActivityUnit,
    decimal EmissionFactorKgCo2e,
    decimal Gwp = 1m);
