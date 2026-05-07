using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.Reports;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Application.Reports;

public sealed class ReportService(
    IReportRepository reportRepository,
    ICurrentTenantAccessor currentTenantAccessor) : IReportService
{
    public async Task<EmissionsDashboardResponse> GetEmissionsDashboardAsync(
        Guid? companyId,
        int? year,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var scopeTotals = await reportRepository.GetScopeTotalsAsync(tenantId, companyId, year, cancellationToken);
        var monthlyTimeline = await reportRepository.GetMonthlyTimelineAsync(tenantId, companyId, year, cancellationToken);
        var totalKg = scopeTotals.Sum(item => item.TotalKgCo2e);
        var totalT = totalKg / 1000m;

        return new EmissionsDashboardResponse(
            tenantId,
            companyId,
            year,
            totalKg,
            totalT,
            Math.Ceiling(totalT),
            scopeTotals,
            monthlyTimeline);
    }
}
