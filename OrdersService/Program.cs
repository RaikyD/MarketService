using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Application.Interfaces;
using OrdersService.Application.Services;
using OrdersService.Infrastructure.DbData;
using OrdersService.Infrastructure.Interfaces;
using OrdersService.Infrastructure.Kafka;
using OrdersService.Infrastructure.Repositories;
using OrdersService.Presentation.Hub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("OrderServiceDB")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ProducerConfig>>().Value);
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ConsumerConfig>>().Value);
builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddHostedService<InboxProcessor>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    var retries = 5;
    while (true)
    {
        try { db.Database.Migrate(); break; }
        catch
        {
            if (--retries == 0) throw;
            Thread.Sleep(2000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdersService v1"));
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<OrderHub>("/orderHub");
});

app.Run();
