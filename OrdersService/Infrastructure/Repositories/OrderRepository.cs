using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.DbData;
using OrdersService.Infrastructure.Interfaces;

namespace OrdersService.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _db;
    public OrderRepository(OrderDbContext db) => _db = db;

    public async Task<Order?> GetOrderAsync(Guid orderId)
        => await _db.Orders.FindAsync(orderId);

    public async Task<List<Order>> GetAllOrdersAsync()
        => await _db.Orders.ToListAsync();

    public async Task AddOrderAsync(Order order)
        => await _db.Orders.AddAsync(order);

    public Task UpdateOrderAsync(Order order)
    {
        _db.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
        => _db.Database.BeginTransactionAsync();

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();

    public async Task AddOutboxMessageAsync(OutboxMessage msg)
        => await _db.Outbox.AddAsync(msg);
    public async Task AddInboxMessageAsync(InboxMessage msg)
    {
        await _db.Inbox.AddAsync(msg);
    }

    public async Task<bool> HasProcessedEventAsync(Guid eventId)
    {
        return await _db.Inbox.AnyAsync(i => i.EventId == eventId);
    }
}