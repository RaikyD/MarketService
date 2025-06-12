using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.DbData;

public class OrderDbContext : DbContext
{
    public DbSet<Order>         Orders { get; set; } = null!;
    public DbSet<OutboxMessage> Outbox { get; set; } = null!;
    public DbSet<InboxMessage>  Inbox  { get; set; } = null!;

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasDefaultSchema("OrderService");

        mb.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.Status)
                .HasConversion<int>()  
                .IsRequired();
        });

        mb.Entity<OutboxMessage>(e =>
        {
            e.ToTable("OutboxMessages");
            e.HasKey(o => o.Id);
        });

        mb.Entity<InboxMessage>(e =>
        {
            e.ToTable("InboxMessages");
            e.HasKey(i => i.EventId);
        });
    }
}