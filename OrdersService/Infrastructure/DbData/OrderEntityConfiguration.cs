using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.DbData;

public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.UserId)
            .IsRequired();
        builder.Property(f => f.Amount)
            .IsRequired();
        builder.Property(f => f.Description);
        builder.Property(f => f.Status)
            .IsRequired();
    }
}