using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class EmissionCategoryConfiguration : IEntityTypeConfiguration<EmissionCategory>
{
    public void Configure(EntityTypeBuilder<EmissionCategory> builder)
    {
        builder.ToTable("emission_categories");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Scope)
            .HasColumnName("scope")
            .HasConversion(
                scope => ConvertScope(scope),
                value => ParseScope(value));
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(120);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(entity => entity.Description).HasColumnName("description");
        builder.Property(entity => entity.ParentCategoryId).HasColumnName("parent_category_id");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }

    private static string ConvertScope(EmissionScope scope)
    {
        return scope switch
        {
            EmissionScope.Scope1 => "scope_1",
            EmissionScope.Scope2 => "scope_2",
            _ => "scope_3"
        };
    }

    private static EmissionScope ParseScope(string value)
    {
        return value switch
        {
            "scope_1" => EmissionScope.Scope1,
            "scope_2" => EmissionScope.Scope2,
            _ => EmissionScope.Scope3
        };
    }
}
