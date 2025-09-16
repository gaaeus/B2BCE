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

        b.OwnsMany(x => x.StateRegistrations, reg =>
        {
            reg.ToTable("StateRegistrations");
            reg.WithOwner().HasForeignKey("CompanyId");

            reg.HasKey(r => r.Id);
            reg.Property(r => r.Id).ValueGeneratedNever();

            reg.Property(r => r.Uf).IsRequired().HasMaxLength(2);
            reg.Property(r => r.Ie).IsRequired().HasMaxLength(32);
            reg.Property<string?>("Status").HasMaxLength(64);
            reg.Property<string?>("RegimeTributario").HasMaxLength(64);
            reg.Property<DateTimeOffset?>("LastCheckedAt");

            reg.HasIndex("CompanyId", "Uf").IsUnique();
        });

    }
}
