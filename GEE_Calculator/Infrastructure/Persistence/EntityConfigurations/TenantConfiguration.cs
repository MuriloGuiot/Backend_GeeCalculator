using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.ExternalTenantId).HasColumnName("external_tenant_id").HasMaxLength(120);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
