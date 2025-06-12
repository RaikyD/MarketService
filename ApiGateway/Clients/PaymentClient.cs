using SharedContacts.Http.Clients.Interfaces;
using SharedContacts.Http.Contracts;
using System.Net.Http.Json;

namespace ApiGateway.Clients;

public class PaymentsClient : IPaymentsClient
{
    private readonly HttpClient _http;
    public PaymentsClient(HttpClient http) => _http = http;

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<decimal>($"/api/accounts/{userId}/balance");
    }

    public async Task TopUpAsync(Guid userId, AdjustBalanceRequest dto)
    {
        var resp = await _http.PostAsJsonAsync($"/api/accounts/{userId}/topup", dto);
        resp.EnsureSuccessStatusCode();
    }

    public async Task WithdrawAsync(Guid userId, AdjustBalanceRequest dto)
    {
        var resp = await _http.PostAsJsonAsync($"/api/accounts/{userId}/withdraw", dto);
        resp.EnsureSuccessStatusCode();
    }
}
