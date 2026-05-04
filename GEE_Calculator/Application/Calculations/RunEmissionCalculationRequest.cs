using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Application.Calculations;

public sealed record RunEmissionCalculationRequest(
    string CompanyLegalName,
    string? CompanyTradeName,
    string? CompanyTaxId,
    string? ExternalCompanyId,
    PeriodType PeriodType,
    int Year,
    int? Month,
    string? FactorSetCode,
    IReadOnlyCollection<RunEmissionCalculationEntryRequest> Entries);
