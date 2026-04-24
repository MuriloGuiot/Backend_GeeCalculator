using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class EmissionInventoryConfiguration : IEntityTypeConfiguration<EmissionInventory>
{
    public void Configure(EntityTypeBuilder<EmissionInventory> builder)
    {
        builder.ToTable("emission_inventories");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.CompanyId).HasColumnName("company_id");
        builder.Property(entity => entity.PeriodType)
            .HasColumnName("period_type")
            .HasConversion(
                period => period == PeriodType.Monthly ? "monthly" : "annual",
                value => value == "monthly" ? PeriodType.Monthly : PeriodType.Annual);
        builder.Property(entity => entity.Year).HasColumnName("year");
        builder.Property(entity => entity.Month).HasColumnName("month");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
