using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class ActivityUnitConfiguration : IEntityTypeConfiguration<ActivityUnit>
{
    public void Configure(EntityTypeBuilder<ActivityUnit> builder)
    {
        builder.ToTable("activity_units");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(40);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120);
        builder.Property(entity => entity.Dimension).HasColumnName("dimension").HasMaxLength(80);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
