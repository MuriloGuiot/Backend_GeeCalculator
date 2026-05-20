using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class SurveyOptionConfiguration : IEntityTypeConfiguration<SurveyOption>
{
    public void Configure(EntityTypeBuilder<SurveyOption> builder)
    {
        builder.ToTable("survey_options");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.QuestionId, entity.Code })
            .IsUnique()
            .HasDatabaseName("uq_survey_options_question_code");
        builder.HasIndex(entity => new { entity.QuestionId, entity.SortOrder })
            .HasDatabaseName("ix_survey_options_question_sort_order");

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.QuestionId).HasColumnName("question_id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(120);
        builder.Property(entity => entity.Label).HasColumnName("label").HasMaxLength(240);
        builder.Property(entity => entity.Value).HasColumnName("value").HasMaxLength(240);
        builder.Property(entity => entity.SortOrder).HasColumnName("sort_order");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");

        builder.HasOne<SurveyQuestion>()
            .WithMany()
            .HasForeignKey(entity => entity.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
