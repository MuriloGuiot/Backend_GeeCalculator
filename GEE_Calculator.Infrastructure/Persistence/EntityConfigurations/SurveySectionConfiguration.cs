using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class SurveySectionConfiguration : IEntityTypeConfiguration<SurveySection>
{
    public void Configure(EntityTypeBuilder<SurveySection> builder)
    {
        builder.ToTable("survey_sections");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.TemplateId, entity.Code })
            .IsUnique()
            .HasDatabaseName("uq_survey_sections_template_code");
        builder.HasIndex(entity => new { entity.TemplateId, entity.SortOrder })
            .HasDatabaseName("ix_survey_sections_template_sort_order");

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TemplateId).HasColumnName("template_id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(120);
        builder.Property(entity => entity.Title).HasColumnName("title").HasMaxLength(240);
        builder.Property(entity => entity.Description).HasColumnName("description");
        builder.Property(entity => entity.SortOrder).HasColumnName("sort_order");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");

        builder.HasOne<SurveyTemplate>()
            .WithMany()
            .HasForeignKey(entity => entity.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
