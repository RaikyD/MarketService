using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentsService.Application.Interfaces;
using PaymentsService.Application.Services;
using PaymentsService.Infrastructure.DbData;
using PaymentsService.Infrastructure.Interfaces;
using PaymentsService.Infrastructure.Kafka;
using PaymentsService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PaymentDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PaymentServiceDB")));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ProducerConfig>>().Value);
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ConsumerConfig>>().Value);

builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddHostedService<InboxProcessor>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    // Ждём подгрузки бд и применяем миграции
    var retries = 5;
    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
