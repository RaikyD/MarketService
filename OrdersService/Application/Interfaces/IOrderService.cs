using OrdersService.Domain.Entities;

namespace OrdersService.Application.Interfaces;

public interface IOrderService
{
    Task<Order>           GetOrderAsync(Guid orderId);
    Task<List<Order>>     GetAllOrdersAsync();
    Task<Guid>            AddOrderAsync(Guid userId, decimal amount, string description = "");
}