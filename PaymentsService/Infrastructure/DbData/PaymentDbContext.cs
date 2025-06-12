using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.DbData;

public class PaymentDbContext : DbContext
{
    public DbSet<UserAccount> Users    { get; set; } = null!;
    public DbSet<OutboxMessage> Outbox { get; set; } = null!;
    public DbSet<InboxMessage>  Inbox  { get; set; } = null!;

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasDefaultSchema("PaymentService");
        mb.ApplyConfiguration(new PaymentEntityConfiguration());
        mb.Entity<OutboxMessage>()
            .ToTable("OutboxMessages")
            .HasKey(x => x.Id);
        mb.Entity<InboxMessage>()
            .ToTable("InboxMessages")
            .HasKey(x => x.EventId);
    }
}