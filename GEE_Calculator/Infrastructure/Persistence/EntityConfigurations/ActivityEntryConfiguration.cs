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

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.InventoryId).HasColumnName("inventory_id");
        builder.Property(entity => entity.CategoryId).HasColumnName("category_id");
        builder.Property(entity => entity.ActivityUnitId).HasColumnName("activity_unit_id");
        builder.Property(entity => entity.ActivityValue).HasColumnName("activity_value").HasPrecision(18, 6);
        builder.Property(entity => entity.EvidenceRef).HasColumnName("evidence_ref");
        builder.Property(entity => entity.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
