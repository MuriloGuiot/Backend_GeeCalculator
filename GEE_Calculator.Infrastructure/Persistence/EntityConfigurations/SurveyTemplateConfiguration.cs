using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class SurveyTemplateConfiguration : IEntityTypeConfiguration<SurveyTemplate>
{
    public void Configure(EntityTypeBuilder<SurveyTemplate> builder)
    {
        builder.ToTable("survey_templates");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => entity.Code)
            .IsUnique()
            .HasDatabaseName("uq_survey_templates_code");
        builder.HasIndex(entity => new { entity.IsActive, entity.Code })
            .HasDatabaseName("ix_survey_templates_active_code");

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(120);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(240);
        builder.Property(entity => entity.VersionLabel).HasColumnName("version_label").HasMaxLength(80);
        builder.Property(entity => entity.FactorSetId).HasColumnName("factor_set_id");
        builder.Property(entity => entity.IsActive).HasColumnName("is_active");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");

        builder.HasOne<EmissionFactorSet>()
            .WithMany()
            .HasForeignKey(entity => entity.FactorSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
