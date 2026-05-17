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
        builder.HasIndex(entity => new { entity.CategoryId, entity.ActivityUnitId, entity.FactorSetId })
            .HasDatabaseName("ix_emission_factors_lookup");
        builder.HasIndex(entity => new { entity.FactorSetId, entity.CategoryId, entity.ActivityUnitId, entity.GasId })
            .HasFilter("tenant_id is null")
            .HasDatabaseName("uq_emission_factors_global_lookup")
            .IsUnique()
            .AreNullsDistinct(false);
        builder.HasIndex(entity => new { entity.TenantId, entity.FactorSetId, entity.CategoryId, entity.ActivityUnitId, entity.GasId })
            .HasFilter("tenant_id is not null")
            .HasDatabaseName("uq_emission_factors_tenant_lookup")
            .IsUnique()
            .AreNullsDistinct(false);

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

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EmissionFactorSet>()
            .WithMany()
            .HasForeignKey(entity => entity.FactorSetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EmissionCategory>()
            .WithMany()
            .HasForeignKey(entity => entity.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActivityUnit>()
            .WithMany()
            .HasForeignKey(entity => entity.ActivityUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GreenhouseGas>()
            .WithMany()
            .HasForeignKey(entity => entity.GasId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_emission_factors_factor_kg_per_unit", "factor_kg_per_unit is null or factor_kg_per_unit >= 0");
            table.HasCheckConstraint("ck_emission_factors_gwp", "gwp is null or gwp > 0");
            table.HasCheckConstraint("ck_emission_factors_factor_kg_co2e", "factor_kg_co2e_per_unit >= 0");
        });
    }
}
