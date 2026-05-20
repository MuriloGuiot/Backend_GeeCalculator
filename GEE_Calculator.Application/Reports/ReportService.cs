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
        var categoryTotals = await reportRepository.GetCategoryTotalsAsync(tenantId, companyId, year, cancellationToken);
        var monthlyTimeline = await reportRepository.GetMonthlyTimelineAsync(tenantId, companyId, year, cancellationToken);
        var totalKg = scopeTotals.Sum(item => item.TotalKgCo2e);
        var totalT = totalKg / 1000m;
        var totalBiogenicKg = scopeTotals.Sum(item => item.BiogenicKgCo2);
        var totalBiogenicRemovalKg = scopeTotals.Sum(item => item.BiogenicRemovalKgCo2);

        return new EmissionsDashboardResponse(
            tenantId,
            companyId,
            year,
            totalKg,
            totalT,
            totalBiogenicKg,
            totalBiogenicKg / 1000m,
            totalBiogenicRemovalKg,
            totalBiogenicRemovalKg / 1000m,
            Math.Ceiling(totalT),
            scopeTotals,
            categoryTotals,
            monthlyTimeline);
    }
}
