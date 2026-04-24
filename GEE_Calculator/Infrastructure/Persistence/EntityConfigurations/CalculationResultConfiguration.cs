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
