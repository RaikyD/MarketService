using System.Text.Json;
using OrdersService.Application.Interfaces;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Interfaces;
using SharedContacts.Events;

namespace OrdersService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;

    public OrderService(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> AddOrderAsync(Guid userId, decimal amount, string description = "")
    {
        // 1. Начинаем транзакцию
        await using var tx = await _repo.BeginTransactionAsync();

        // 2. Создаём агрегат
        var order = new Order(userId, amount, description);
        await _repo.AddOrderAsync(order);

        // 3. Кладём событие в Outbox
        var evt = new OrderCreatedEvent
        {
            EventId    = Guid.NewGuid(),
            OrderId    = order.Id,
            UserId     = order.UserId,
            Amount     = order.Amount,
            OccurredOn = DateTime.UtcNow
        };
        var outbox = new OutboxMessage
        {
            Id         = evt.EventId,
            Topic      = "order-created",
            Payload    = JsonSerializer.Serialize(evt),
            OccurredOn = evt.OccurredOn,
            Dispatched = false
        };
        await _repo.AddOutboxMessageAsync(outbox);

        // 4. Сохраняем и коммитим
        await _repo.SaveChangesAsync();
        await tx.CommitAsync();

        return order.Id;
    }

    public async Task<Order> GetOrderAsync(Guid orderId)
    {
        var o = await _repo.GetOrderAsync(orderId);
        if (o == null) throw new KeyNotFoundException($"Order {orderId} not found");
        return o;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _repo.GetAllOrdersAsync();
    }
}