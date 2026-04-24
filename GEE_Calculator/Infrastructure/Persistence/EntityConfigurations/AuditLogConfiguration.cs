using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.ActorExternalUserId).HasColumnName("actor_external_user_id").HasMaxLength(160);
        builder.Property(entity => entity.Action).HasColumnName("action").HasMaxLength(120);
        builder.Property(entity => entity.EntityName).HasColumnName("entity_name").HasMaxLength(120);
        builder.Property(entity => entity.EntityId).HasColumnName("entity_id").HasMaxLength(80);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
