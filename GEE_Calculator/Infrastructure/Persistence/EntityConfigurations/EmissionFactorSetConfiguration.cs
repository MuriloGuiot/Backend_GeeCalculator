using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class EmissionFactorSetConfiguration : IEntityTypeConfiguration<EmissionFactorSet>
{
    public void Configure(EntityTypeBuilder<EmissionFactorSet> builder)
    {
        builder.ToTable("emission_factor_sets");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.SourceId).HasColumnName("source_id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(120);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(240);
        builder.Property(entity => entity.VersionLabel).HasColumnName("version_label").HasMaxLength(80);
        builder.Property(entity => entity.VersionYear).HasColumnName("version_year");
        builder.Property(entity => entity.ValidFrom).HasColumnName("valid_from");
        builder.Property(entity => entity.ValidTo).HasColumnName("valid_to");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
