using BuildingBlocks.Domain.Companies;
using BuildingBlocks.Domain.Companies.Entities;
using BuildingBlocks.Domain.Companies.ValueObjects;
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

        // optional provenance / canonical info (if present in your imported Company.cs)
        if (typeof(Company).GetProperty("CanonicalSource") != null)
        {
            b.Property<string?>("CanonicalSource").HasMaxLength(100);
            b.Property<string?>("CanonicalVersion").HasMaxLength(50);
        }

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

        // Establishments (branches) mapping if present
        if (typeof(Company).GetProperty("Establishments") != null)
        {
            b.OwnsMany(typeof(Establishment), "Establishments", est =>
            {
                est.ToTable("Establishments");
                est.WithOwner().HasForeignKey("CompanyId");
                est.HasKey("Id");
                est.Property<Guid>("Id").ValueGeneratedNever();
                // Add appropriate properties mapping according to your Establishment fields:
                est.Property<string>("Name").HasMaxLength(200);
                est.Property<string?>("Address").HasMaxLength(500);
                est.OwnsOne(typeof(Address), "Address", addr =>
                {
                    addr.Property<string>("Street").HasMaxLength(200);
                    addr.Property<string>("Number").HasMaxLength(50);
                    addr.Property<string?>("Complement").HasMaxLength(150);
                    addr.Property<string>("District").HasMaxLength(100);
                    addr.Property<string>("City").HasMaxLength(100);
                    addr.Property<string>("State").HasMaxLength(2);
                    addr.Property<string>("ZipCode").HasMaxLength(15);
                    addr.Property<string>("Country").HasMaxLength(50);
                });
                // add more mappings if your Establishment has other scalar props
            });
        }

        // ContactPersons mapping if present
        if (typeof(Company).GetProperty("ContactPersons") != null)
        {
            b.OwnsMany(typeof(ContactPerson), "ContactPersons", cp =>
            {
                cp.ToTable("ContactPersons");
                cp.WithOwner().HasForeignKey("CompanyId");
                cp.HasKey("Id");
                cp.Property<Guid>("Id").ValueGeneratedNever();
                cp.Property<string>("FullName").HasMaxLength(200);
                cp.Property<string?>("Email").HasMaxLength(320);
                cp.Property<string?>("Phone").HasMaxLength(50);
            });
        }

        // Ignore domain events if you use them
        b.Ignore(x => x.DomainEvents);
    }
}
