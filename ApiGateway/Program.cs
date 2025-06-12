using ApiGateway;
using ApiGateway.Clients;
using SharedContacts.Http.Clients.Interfaces;
using Microsoft.Extensions.Options;

// ХЗ тут пока просто бред написан, позже буду менять ещё, когда доведу работу сервисов до безотказности
var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<GatewayOptions>(builder.Configuration.GetSection("Services"));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<GatewayOptions>>().Value);


builder.Services.AddHttpClient<IPaymentsClient, PaymentsClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<GatewayOptions>();
    client.BaseAddress = new Uri(opts.Payments);
});
builder.Services.AddHttpClient<IOrdersClient, OrdersClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<GatewayOptions>();
    client.BaseAddress = new Uri(opts.Orders);
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
