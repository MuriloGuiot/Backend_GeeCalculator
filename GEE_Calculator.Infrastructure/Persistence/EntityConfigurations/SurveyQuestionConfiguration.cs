using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
    public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
    {
        builder.ToTable("survey_questions");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.SectionId, entity.Code })
            .IsUnique()
            .HasDatabaseName("uq_survey_questions_section_code");
        builder.HasIndex(entity => new { entity.SectionId, entity.SortOrder })
            .HasDatabaseName("ix_survey_questions_section_sort_order");
        builder.HasIndex(entity => entity.VisibilityRuleJson)
            .HasMethod("gin")
            .HasDatabaseName("ix_survey_questions_visibility_rule");
        builder.HasIndex(entity => entity.MappingJson)
            .HasMethod("gin")
            .HasDatabaseName("ix_survey_questions_mapping");

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.SectionId).HasColumnName("section_id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(160);
        builder.Property(entity => entity.Prompt).HasColumnName("prompt").HasMaxLength(500);
        builder.Property(entity => entity.HelpText).HasColumnName("help_text");
        builder.Property(entity => entity.AnswerType).HasColumnName("answer_type").HasMaxLength(40);
        builder.Property(entity => entity.IsRequired).HasColumnName("is_required");
        builder.Property(entity => entity.SortOrder).HasColumnName("sort_order");
        builder.Property(entity => entity.VisibilityRuleJson).HasColumnName("visibility_rule").HasColumnType("jsonb").HasDefaultValue("{}");
        builder.Property(entity => entity.MappingJson).HasColumnName("mapping").HasColumnType("jsonb").HasDefaultValue("{}");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");

        builder.HasOne<SurveySection>()
            .WithMany()
            .HasForeignKey(entity => entity.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
