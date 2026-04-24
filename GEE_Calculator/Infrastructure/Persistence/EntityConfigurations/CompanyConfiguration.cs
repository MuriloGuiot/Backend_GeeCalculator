using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEE_Calculator.Infrastructure.Persistence.EntityConfigurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id");
        builder.Property(entity => entity.LegalName).HasColumnName("legal_name").HasMaxLength(240);
        builder.Property(entity => entity.TradeName).HasColumnName("trade_name").HasMaxLength(240);
        builder.Property(entity => entity.TaxId).HasColumnName("tax_id").HasMaxLength(32);
        builder.Property(entity => entity.ExternalCompanyId).HasColumnName("external_company_id").HasMaxLength(120);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
    }
}
