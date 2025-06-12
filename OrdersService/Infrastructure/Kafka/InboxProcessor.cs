using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedContacts.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Interfaces;

namespace OrdersService.Infrastructure.Kafka;

public class InboxProcessor : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ConsumerConfig   _cfg;

    public InboxProcessor(IServiceProvider sp, ConsumerConfig cfg)
    {
        _sp  = sp;
        _cfg = cfg;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_cfg).Build();
        consumer.Subscribe("payment-completed");

        try
        {
            while (!ct.IsCancellationRequested)
            {
                ConsumeResult<Ignore,string> cr;
                try
                {
                    cr = consumer.Consume(ct);
                }
                catch (Exception ex)
                {
                    await Task.Delay(1000, ct);
                    continue;
                }

                var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(cr.Message.Value);
                if (evt == null)
                {
                    consumer.Commit(cr);
                    continue;
                }

                using var scope = _sp.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                // идемпотентность
                if (await repo.HasProcessedEventAsync(evt.EventId))
                {
                    consumer.Commit(cr);
                    continue;
                }

                await using var tx = await repo.BeginTransactionAsync();

                await repo.AddInboxMessageAsync(new InboxMessage
                {
                    EventId    = evt.EventId,
                    Topic      = "payment-completed",
                    ReceivedOn = DateTime.UtcNow
                });

                var order = await repo.GetOrderAsync(evt.OrderId)
                          ?? throw new KeyNotFoundException($"Order {evt.OrderId} not found");
                order.MarkFinished();
                await repo.UpdateOrderAsync(order);

                await repo.SaveChangesAsync();
                await tx.CommitAsync();

                consumer.Commit(cr);
            }
        }
        catch (OperationCanceledException) { }
        finally { consumer.Close(); }
    }
}
