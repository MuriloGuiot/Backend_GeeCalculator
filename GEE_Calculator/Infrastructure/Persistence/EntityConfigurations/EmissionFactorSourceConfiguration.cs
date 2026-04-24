using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class EmissionFactorSourceConfiguration : IEntityTypeConfiguration<EmissionFactorSource>
{
    public void Configure(EntityTypeBuilder<EmissionFactorSource> builder)
    {
        builder.ToTable("emission_factor_sources");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(80);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(240);
        builder.Property(entity => entity.Publisher).HasColumnName("publisher").HasMaxLength(240);
        builder.Property(entity => entity.SourceUrl).HasColumnName("source_url");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
