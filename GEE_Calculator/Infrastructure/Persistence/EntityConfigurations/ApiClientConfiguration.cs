using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
{
    public void Configure(EntityTypeBuilder<ApiClient> builder)
    {
        builder.ToTable("api_clients");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(160);
        builder.Property(entity => entity.KeyPrefix).HasColumnName("key_prefix").HasMaxLength(24);
        builder.Property(entity => entity.KeyHash).HasColumnName("key_hash").HasMaxLength(160);
        builder.Property(entity => entity.IsActive).HasColumnName("is_active");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.RevokedAt).HasColumnName("revoked_at");
    }
}
