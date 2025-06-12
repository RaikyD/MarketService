using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using OrdersService.Infrastructure.DbData;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrdersService.Infrastructure.Kafka;

public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly KafkaProducer   _producer;

    public OutboxDispatcher(IServiceProvider sp, KafkaProducer producer)
    {
        _sp       = sp;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var pending = await db.Outbox
                .Where(x => !x.Dispatched)
                .ToListAsync(ct);

            foreach (var msg in pending)
            {
                //для дебага 
                Console.WriteLine($"TEST - {msg.Topic}, {msg.Payload}");
                Console.WriteLine($"TEST - {msg.Topic}, {msg.Payload}");
                Console.WriteLine($"TEST - {msg.Topic}, {msg.Payload}");
                Console.WriteLine($"TEST - {msg.Topic}, {msg.Payload}");
                Console.WriteLine($"TEST - {msg.Topic}, {msg.Payload}");
                Console.WriteLine($"TEST - {msg.Topic}, {msg.Payload}");
                await _producer.ProduceAsync(msg.Topic, msg.Payload);
                msg.Dispatched = true;
            }

            await db.SaveChangesAsync(ct);
            await Task.Delay(1000, ct);
        }
    }
}