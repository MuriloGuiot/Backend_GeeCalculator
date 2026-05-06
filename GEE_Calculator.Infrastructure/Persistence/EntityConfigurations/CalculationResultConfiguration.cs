using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class CalculationResultConfiguration : IEntityTypeConfiguration<CalculationResult>
{
    public void Configure(EntityTypeBuilder<CalculationResult> builder)
    {
        builder.ToTable("calculation_results");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.TenantId, entity.CalculationRunId })
            .HasDatabaseName("ix_calculation_results_tenant_run");
        builder.HasIndex(entity => new { entity.Scope, entity.CategoryId })
            .HasDatabaseName("ix_calculation_results_scope_category");

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.CalculationRunId).HasColumnName("calculation_run_id");
        builder.Property(entity => entity.Scope)
            .HasColumnName("scope")
            .HasConversion(
                scope => ConvertScope(scope),
                value => ParseScope(value));
        builder.Property(entity => entity.CategoryId).HasColumnName("category_id");
        builder.Property(entity => entity.GasId).HasColumnName("gas_id");
        builder.Property(entity => entity.TotalKgCo2e).HasColumnName("total_kg_co2e").HasPrecision(18, 6);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Ignore(entity => entity.TotalTCo2e);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CalculationRun>()
            .WithMany()
            .HasForeignKey(entity => entity.CalculationRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EmissionCategory>()
            .WithMany()
            .HasForeignKey(entity => entity.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GreenhouseGas>()
            .WithMany()
            .HasForeignKey(entity => entity.GasId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_calculation_results_total_kg_co2e", "total_kg_co2e >= 0");
        });
    }

    private static string ConvertScope(EmissionScope scope)
    {
        return scope switch
        {
            EmissionScope.Scope1 => "scope_1",
            EmissionScope.Scope2 => "scope_2",
            _ => "scope_3"
        };
    }

    private static EmissionScope ParseScope(string value)
    {
        return value switch
        {
            "scope_1" => EmissionScope.Scope1,
            "scope_2" => EmissionScope.Scope2,
            _ => EmissionScope.Scope3
        };
    }
}
