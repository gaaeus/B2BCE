using BuildingBlocks.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Persistence.Configurations
{
    internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> b)
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(256);
            b.Property(x => x.Payload).IsRequired();           // JSON
            b.Property(x => x.OccurredOnUtc).IsRequired();
            b.Property(x => x.ProcessedOnUtc);
            b.Property(x => x.Error);
            b.Property(x => x.LockedAtUtc);
            b.Property(x => x.LockedBy).HasMaxLength(64);

            b.HasIndex(x => new { x.ProcessedOnUtc, x.LockedAtUtc });
        }
    }
}
