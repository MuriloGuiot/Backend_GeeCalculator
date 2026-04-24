using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class GreenhouseGasConfiguration : IEntityTypeConfiguration<GreenhouseGas>
{
    public void Configure(EntityTypeBuilder<GreenhouseGas> builder)
    {
        builder.ToTable("greenhouse_gases");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(24);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120);
        builder.Property(entity => entity.DefaultGwp).HasColumnName("default_gwp").HasPrecision(18, 8);
        builder.Property(entity => entity.GwpSource).HasColumnName("gwp_source").HasMaxLength(240);
        builder.Property(entity => entity.VersionYear).HasColumnName("version_year");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
