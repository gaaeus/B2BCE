using BuildingBlocks.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.ToTable("Companies");
        b.HasKey(x => x.Id);

        b.Property(x => x.LegalName).IsRequired().HasMaxLength(250);

        b.OwnsOne(x => x.TaxId, tax =>
        {
            tax.Property(p => p.Value).HasColumnName("TaxId").IsRequired().HasMaxLength(20);
            tax.HasIndex(p => p.Value).IsUnique();
        });

        b.OwnsOne(x => x.Email, email =>
        {
            email.Property(p => p.Value).HasColumnName("Email").HasMaxLength(320);
        });

        b.Ignore(x => x.DomainEvents);
    }
}
