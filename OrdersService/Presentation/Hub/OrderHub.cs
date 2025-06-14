namespace OrdersService.Presentation.Hub;

public class OrderHub : Microsoft.AspNetCore.SignalR.Hub
{
    public Task JoinOrder(string orderId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, orderId);
}