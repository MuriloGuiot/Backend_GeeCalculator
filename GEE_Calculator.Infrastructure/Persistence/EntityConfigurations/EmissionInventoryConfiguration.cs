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
        builder.HasIndex(entity => new { entity.TenantId, entity.CompanyId })
            .HasDatabaseName("ix_emission_inventories_tenant_company");
        builder.HasIndex(entity => new { entity.TenantId, entity.CompanyId, entity.PeriodType, entity.Year, entity.Month })
            .HasFilter("period_type = 'monthly'")
            .HasDatabaseName("uq_emission_inventories_period_monthly")
            .IsUnique();
        builder.HasIndex(entity => new { entity.TenantId, entity.CompanyId, entity.PeriodType, entity.Year })
            .HasFilter("period_type = 'annual'")
            .HasDatabaseName("uq_emission_inventories_period_annual")
            .IsUnique();

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

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(entity => entity.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_emission_inventories_year", "year between 2000 and 2100");
            table.HasCheckConstraint("ck_emission_inventories_month", "month is null or month between 1 and 12");
            table.HasCheckConstraint("ck_emission_inventories_period_month",
                "(period_type = 'monthly' and month is not null) or (period_type = 'annual' and month is null)");
        });
    }
}
