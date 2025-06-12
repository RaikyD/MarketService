using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Interfaces;

public interface IOrderRepository
{
    Task<Order?>    GetOrderAsync(Guid orderId);
    Task<List<Order>> GetAllOrdersAsync();

    Task AddOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);

    // Транзакционный API
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();

    // Outbox
    Task AddOutboxMessageAsync(OutboxMessage msg);

    // **Inbox**
    /// <summary>Записать в Inbox факт обработки события</summary>
    Task AddInboxMessageAsync(InboxMessage msg);

    /// <summary>Проверить, было ли событие с таким EventId уже обработано</summary>
    Task<bool> HasProcessedEventAsync(Guid eventId);
}