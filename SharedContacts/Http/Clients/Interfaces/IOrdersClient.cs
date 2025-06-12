using SharedContacts.Http.Contracts;

namespace SharedContacts.Http.Clients.Interfaces;

public interface IOrdersClient
{
    Task<Guid> CreateOrderAsync(CreateOrderRequest dto);
    Task<OrderDto> GetOrderAsync(Guid orderId);
    Task<List<OrderDto>> GetAllOrdersAsync();
}