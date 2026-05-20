using GEE_Calculator.Application.Reports;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Reports;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Tests;

public sealed class ReportServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetEmissionsDashboardAsync_ShouldAggregateScopesAndCredits()
    {
        var service = new ReportService(new FixedReportRepository(), new FixedTenantAccessor());

        var response = await service.GetEmissionsDashboardAsync(companyId: null, year: 2026);

        Assert.Equal(593.75m, response.TotalKgCo2e);
        Assert.Equal(0.59375m, response.TotalTCo2e);
        Assert.Equal(25m, response.TotalBiogenicKgCo2);
        Assert.Equal(5m, response.TotalBiogenicRemovalKgCo2);
        Assert.Equal(1m, response.CarbonCreditsRequired);
        Assert.Equal(2, response.Scopes.Count);
        Assert.Single(response.Categories);
    }

    private sealed class FixedTenantAccessor : ICurrentTenantAccessor
    {
        public CurrentTenantSnapshot GetCurrentTenant()
        {
            return new CurrentTenantSnapshot(TenantId.ToString(), null, null, true);
        }
    }

    private sealed class FixedReportRepository : IReportRepository
    {
        public Task<IReadOnlyCollection<ScopeDashboardItem>> GetScopeTotalsAsync(
            Guid tenantId,
            Guid? companyId,
            int? year,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<ScopeDashboardItem>>(
            [
                new ScopeDashboardItem(EmissionScope.Scope1, 536m, 0.536m, 25m, 0.025m, 5m, 0.005m),
                new ScopeDashboardItem(EmissionScope.Scope2, 57.75m, 0.05775m, 0m, 0m, 0m, 0m)
            ]);
        }

        public Task<IReadOnlyCollection<CategoryDashboardItem>> GetCategoryTotalsAsync(
            Guid tenantId,
            Guid? companyId,
            int? year,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<CategoryDashboardItem>>(
            [
                new CategoryDashboardItem(EmissionScope.Scope1, "diesel_rodoviario", "Diesel rodoviario", 536m, 0.536m, 25m, 0.025m, 5m, 0.005m)
            ]);
        }

        public Task<IReadOnlyCollection<MonthlyEmissionsItem>> GetMonthlyTimelineAsync(
            Guid tenantId,
            Guid? companyId,
            int? year,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<MonthlyEmissionsItem>>(
            [
                new MonthlyEmissionsItem(2026, 5, 593.75m, 0.59375m, 25m, 0.025m, 5m, 0.005m)
            ]);
        }
    }
}
