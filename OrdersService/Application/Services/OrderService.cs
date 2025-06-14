using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OrdersService.Application.Interfaces;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Interfaces;
using OrdersService.Presentation.Hub;
using SharedContacts.Events;

namespace OrdersService.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository       _repo;
        private readonly IHubContext<OrderHub>  _hub;

        public OrderService(IOrderRepository repo, IHubContext<OrderHub> hub)
        {
            _repo = repo;
            _hub  = hub;
        }

        public async Task<Guid> AddOrderAsync(Guid userId, decimal amount, string description = "")
        {
            await using var tx = await _repo.BeginTransactionAsync();

            var order = new Order(userId, amount, description);
            await _repo.AddOrderAsync(order);

            // записываем событие в исходящий аутбокс
            var evt = new OrderCreatedEvent {
                EventId    = Guid.NewGuid(),
                OrderId    = order.Id,
                UserId     = order.UserId,
                Amount     = order.Amount,
                OccurredOn = DateTime.UtcNow
            };
            var outbox = new OutboxMessage {
                Id         = evt.EventId,
                Topic      = "order-created",
                Payload    = JsonSerializer.Serialize(evt),
                OccurredOn = evt.OccurredOn,
                Dispatched = false
            };
            await _repo.AddOutboxMessageAsync(outbox);

            await _repo.SaveChangesAsync();
            await tx.CommitAsync();

            await _hub.Clients.Group(order.Id.ToString())
                      .SendAsync("StatusChanged", order.Id, order.Status.ToString());

            return order.Id;
        }

        public async Task<Order> GetOrderAsync(Guid orderId)
        {
            var o = await _repo.GetOrderAsync(orderId);
            if (o == null) throw new KeyNotFoundException($"Order {orderId} not found");
            return o;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
            => await _repo.GetAllOrdersAsync();

        public async Task MarkFinishedAsync(Guid orderId)
        {
            await using var tx = await _repo.BeginTransactionAsync();
            var order = await GetOrderAsync(orderId);
            order.MarkFinished();
            await _repo.UpdateOrderAsync(order);
            await _repo.SaveChangesAsync();
            await tx.CommitAsync();

            await _hub.Clients.Group(orderId.ToString())
                      .SendAsync("StatusChanged", orderId, order.Status.ToString());
        }

        public async Task MarkCanceledAsync(Guid orderId)
        {
            await using var tx = await _repo.BeginTransactionAsync();
            var order = await GetOrderAsync(orderId);
            order.MarkCanceled();
            await _repo.UpdateOrderAsync(order);
            await _repo.SaveChangesAsync();
            await tx.CommitAsync();

            await _hub.Clients.Group(orderId.ToString())
                      .SendAsync("StatusChanged", orderId, order.Status.ToString());
        }
    }
}
