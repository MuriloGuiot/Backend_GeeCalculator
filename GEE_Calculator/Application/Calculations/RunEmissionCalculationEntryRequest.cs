namespace GEE_Calculator.Application.Calculations;

public sealed record RunEmissionCalculationEntryRequest(
    string CategoryCode,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? EvidenceRef,
    string? MetadataJson);
