namespace GEE_Calculator.Domain.Calculations;

public sealed record RunEmissionCalculationEntryRequest(
    string CategoryCode,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? EvidenceRef,
    string? MetadataJson);
