using BuildingBlocks.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Cnpj).HasMaxLength(14);
        builder.Property(x => x.ContactEmail).HasMaxLength(255);
        builder.Property(x => x.SefazEnvironment).HasMaxLength(50);
        builder.Property(x => x.ConnectionString).HasMaxLength(1024);
        builder.Property(x => x.SefazApiKey).HasMaxLength(512);
    }
}
