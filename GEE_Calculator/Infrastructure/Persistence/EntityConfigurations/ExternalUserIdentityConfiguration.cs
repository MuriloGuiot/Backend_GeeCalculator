using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class ExternalUserIdentityConfiguration : IEntityTypeConfiguration<ExternalUserIdentity>
{
    public void Configure(EntityTypeBuilder<ExternalUserIdentity> builder)
    {
        builder.ToTable("external_user_identities");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.Provider).HasColumnName("provider").HasMaxLength(80);
        builder.Property(entity => entity.ExternalUserId).HasColumnName("external_user_id").HasMaxLength(160);
        builder.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(320);
        builder.Property(entity => entity.DisplayName).HasColumnName("display_name").HasMaxLength(200);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
