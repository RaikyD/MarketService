using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Interfaces;

public interface IOrderRepository
{
    Task<Order?>    GetOrderAsync(Guid orderId);
    Task<List<Order>> GetAllOrdersAsync();

    Task AddOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);

    
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();

    
    Task AddOutboxMessageAsync(OutboxMessage msg);

    
    Task AddInboxMessageAsync(InboxMessage msg);
    
    Task<bool> HasProcessedEventAsync(Guid eventId);
}