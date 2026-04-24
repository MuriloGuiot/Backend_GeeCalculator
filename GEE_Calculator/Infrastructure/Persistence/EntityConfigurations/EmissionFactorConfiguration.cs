using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class EmissionFactorConfiguration : IEntityTypeConfiguration<EmissionFactor>
{
    public void Configure(EntityTypeBuilder<EmissionFactor> builder)
    {
        builder.ToTable("emission_factors");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.FactorSetId).HasColumnName("factor_set_id");
        builder.Property(entity => entity.CategoryId).HasColumnName("category_id");
        builder.Property(entity => entity.ActivityUnitId).HasColumnName("activity_unit_id");
        builder.Property(entity => entity.GasId).HasColumnName("gas_id");
        builder.Property(entity => entity.FactorKgPerUnit).HasColumnName("factor_kg_per_unit").HasPrecision(18, 8);
        builder.Property(entity => entity.Gwp).HasColumnName("gwp").HasPrecision(18, 8);
        builder.Property(entity => entity.FactorKgCo2ePerUnit).HasColumnName("factor_kg_co2e_per_unit").HasPrecision(18, 8);
        builder.Property(entity => entity.CalculationNotes).HasColumnName("calculation_notes");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
