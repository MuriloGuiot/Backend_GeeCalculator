using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.Abstractions;
using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence;

public sealed class GeeCalculatorDbContext(
    DbContextOptions<GeeCalculatorDbContext> options,
    ICurrentTenantAccessor currentTenantAccessor) : DbContext(options)
{
    public DbSet<ActivityEntry> ActivityEntries => Set<ActivityEntry>();
    public DbSet<ActivityUnit> ActivityUnits => Set<ActivityUnit>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CalculationResult> CalculationResults => Set<CalculationResult>();
    public DbSet<CalculationRun> CalculationRuns => Set<CalculationRun>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<EmissionCategory> EmissionCategories => Set<EmissionCategory>();
    public DbSet<EmissionFactor> EmissionFactors => Set<EmissionFactor>();
    public DbSet<EmissionFactorSet> EmissionFactorSets => Set<EmissionFactorSet>();
    public DbSet<EmissionFactorSource> EmissionFactorSources => Set<EmissionFactorSource>();
    public DbSet<EmissionInventory> EmissionInventories => Set<EmissionInventory>();
    public DbSet<ExternalUserIdentity> ExternalUserIdentities => Set<ExternalUserIdentity>();
    public DbSet<GreenhouseGas> GreenhouseGases => Set<GreenhouseGas>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeeCalculatorDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ValidateTenantConsistency();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateTenantConsistency();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ValidateTenantConsistency()
    {
        var tenantHeaderValue = currentTenantAccessor.GetCurrentTenant().TenantId;

        if (string.IsNullOrWhiteSpace(tenantHeaderValue) || !Guid.TryParse(tenantHeaderValue, out var tenantId))
        {
            return;
        }

        var invalidEntries = ChangeTracker.Entries<ITenantOwnedEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Where(entry => entry.Entity.TenantId != tenantId)
            .ToArray();

        if (invalidEntries.Length > 0)
        {
            throw new InvalidOperationException("One or more tenant-owned entities do not match the current tenant context.");
        }
    }
}
