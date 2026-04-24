using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class CalculationRunConfiguration : IEntityTypeConfiguration<CalculationRun>
{
    public void Configure(EntityTypeBuilder<CalculationRun> builder)
    {
        builder.ToTable("calculation_runs");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.InventoryId).HasColumnName("inventory_id");
        builder.Property(entity => entity.FactorSetId).HasColumnName("factor_set_id");
        builder.Property(entity => entity.CalculationVersion).HasColumnName("calculation_version").HasMaxLength(40);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
