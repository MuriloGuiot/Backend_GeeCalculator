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
        builder.HasIndex(entity => new { entity.Code, entity.VersionLabel }).IsUnique();
        builder.HasIndex(entity => new { entity.SourceId, entity.VersionYear });

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.SourceId).HasColumnName("source_id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(120);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(240);
        builder.Property(entity => entity.VersionLabel).HasColumnName("version_label").HasMaxLength(80);
        builder.Property(entity => entity.VersionYear).HasColumnName("version_year");
        builder.Property(entity => entity.ValidFrom).HasColumnName("valid_from");
        builder.Property(entity => entity.ValidTo).HasColumnName("valid_to");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");

        builder.HasOne<EmissionFactorSource>()
            .WithMany()
            .HasForeignKey(entity => entity.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_emission_factor_sets_version_year", "version_year between 2000 and 2100");
            table.HasCheckConstraint("ck_emission_factor_sets_valid_range", "valid_to is null or valid_from is null or valid_to >= valid_from");
        });
    }
}
