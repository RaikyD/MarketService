using SharedContacts.Http.Clients.Interfaces;
using SharedContacts.Http.Contracts;

namespace ApiGateway.Clients;

public class OrdersClient : IOrdersClient
{
    private readonly HttpClient _http;
    public OrdersClient(HttpClient http) => _http = http;

    public async Task<Guid> CreateOrderAsync(CreateOrderRequest dto)
    {
        var resp = await _http.PostAsJsonAsync("/api/orders", dto);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Guid>();
    }

    public Task<OrderDto> GetOrderAsync(Guid orderId)
        => _http.GetFromJsonAsync<OrderDto>($"/api/orders/{orderId}");

    public Task<List<OrderDto>> GetAllOrdersAsync()
        => _http.GetFromJsonAsync<List<OrderDto>>("/api/orders");
}