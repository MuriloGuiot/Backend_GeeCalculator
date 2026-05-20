namespace GEE_Calculator.Domain.Calculations;

public sealed record RunEmissionCalculationEntryRequest(
    string CategoryCode,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? SourceName,
    string? CalculationMethod,
    string? EvidenceRef,
    string? MetadataJson);
