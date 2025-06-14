// using Confluent.Kafka;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;
// using OrdersService.Application.Interfaces;
// using OrdersService.Application.Services;
// using OrdersService.Infrastructure.DbData;
// using OrdersService.Infrastructure.Interfaces;
// using OrdersService.Infrastructure.Kafka;
// using OrdersService.Infrastructure.Repositories;
//
// var builder = WebApplication.CreateBuilder(args);
//
// builder.Services.AddDbContext<OrderDbContext>(opt =>
//     opt.UseNpgsql(builder.Configuration.GetConnectionString("OrderServiceDB")));
//
// builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// builder.Services.AddScoped<IOrderService, OrderService>();
//
// builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));
// builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ProducerConfig>>().Value);
// builder.Services.AddSingleton<KafkaProducer>();  
//
// builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka"));
// builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ConsumerConfig>>().Value);
//
// builder.Services.AddHostedService<OutboxDispatcher>();
// builder.Services.AddHostedService<InboxProcessor>();
//
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
//
// var app = builder.Build();
//
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
//     var retries = 5;
//     while (true)
//     {
//         try
//         {
//             db.Database.Migrate();
//             break;
//         }
//         catch
//         {
//             if (--retries == 0) throw;
//             Thread.Sleep(2000);
//         }
//     }
// }
//
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdersService v1"));
// }
//
// app.UseHttpsRedirection();
// app.UseAuthorization();
// app.MapControllers();
// app.Run();

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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrdersService.Presentation.Hub;

var builder = WebApplication.CreateBuilder(args);

// 1) EF Core + репозиторий
builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("OrderServiceDB")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 2) SignalR + OrderService
builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderService, OrderService>();

// 3) Kafka Outbox/Inbox
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ProducerConfig>>().Value);
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ConsumerConfig>>().Value);
builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddHostedService<InboxProcessor>();

// 4) Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Миграции
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

// 5) Маршруты контроллеров и SignalR-хаб
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<OrderHub>("/orderHub");
});

app.Run();
