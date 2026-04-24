using GEE_Calculator.Application.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GEE_Calculator.Infrastructure.Persistence;

public sealed class GeeCalculatorDbContextFactory : IDesignTimeDbContextFactory<GeeCalculatorDbContext>
{
    public GeeCalculatorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GeeCalculatorDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=gee_calculator;Username=gee_user;Password=gee_password");

        return new GeeCalculatorDbContext(optionsBuilder.Options, new DesignTimeTenantAccessor());
    }

    private sealed class DesignTimeTenantAccessor : ICurrentTenantAccessor
    {
        public CurrentTenantSnapshot GetCurrentTenant()
        {
            return new CurrentTenantSnapshot(
                TenantId: null,
                CompanyId: null,
                ApiKeyPrefix: null,
                IsResolved: false);
        }
    }
}
