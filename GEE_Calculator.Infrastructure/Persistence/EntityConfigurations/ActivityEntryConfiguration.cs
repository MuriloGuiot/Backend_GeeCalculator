using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class ActivityEntryConfiguration : IEntityTypeConfiguration<ActivityEntry>
{
    public void Configure(EntityTypeBuilder<ActivityEntry> builder)
    {
        builder.ToTable("activity_entries");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.TenantId, entity.InventoryId })
            .HasDatabaseName("ix_activity_entries_tenant_inventory");
        builder.HasIndex(entity => entity.CategoryId)
            .HasDatabaseName("ix_activity_entries_category");
        builder.HasIndex(entity => new { entity.TenantId, entity.DeletedAt })
            .HasDatabaseName("ix_activity_entries_tenant_deleted_at");
        builder.HasIndex(entity => entity.MetadataJson)
            .HasMethod("gin")
            .HasDatabaseName("ix_activity_entries_metadata");

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.InventoryId).HasColumnName("inventory_id");
        builder.Property(entity => entity.CategoryId).HasColumnName("category_id");
        builder.Property(entity => entity.ActivityUnitId).HasColumnName("activity_unit_id");
        builder.Property(entity => entity.ActivityValue).HasColumnName("activity_value").HasPrecision(18, 6);
        builder.Property(entity => entity.SourceName).HasColumnName("source_name").HasMaxLength(240);
        builder.Property(entity => entity.CalculationMethod).HasColumnName("calculation_method").HasMaxLength(80).HasDefaultValue("factor");
        builder.Property(entity => entity.EvidenceRef).HasColumnName("evidence_ref");
        builder.Property(entity => entity.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(entity => entity.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EmissionInventory>()
            .WithMany()
            .HasForeignKey(entity => entity.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EmissionCategory>()
            .WithMany()
            .HasForeignKey(entity => entity.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActivityUnit>()
            .WithMany()
            .HasForeignKey(entity => entity.ActivityUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_activity_entries_activity_value", "activity_value >= 0");
        });
    }
}
