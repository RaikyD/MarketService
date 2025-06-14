using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsService.Infrastructure.DbData;

namespace PaymentsService.Infrastructure.Kafka
{
    /// <summary>
    /// Фоновый сервис: раз в секунду берёт из таблицы OutboxMessages
    /// несозданные события и шлёт их в Kafka.
    /// </summary>
    public class OutboxDispatcher : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly KafkaProducer   _producer;
        private readonly ILogger<OutboxDispatcher> _logger;

        public OutboxDispatcher(IServiceProvider sp, KafkaProducer producer, ILogger<OutboxDispatcher> logger)
        {
            _sp       = sp;
            _producer = producer;
            _logger   = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                    var pending = await db.Outbox
                        .Where(x => !x.Dispatched)
                        .OrderBy(x => x.OccurredOn)
                        .ToListAsync(ct);

                    foreach (var msg in pending)
                    {
                        try
                        {
                            await _producer.ProduceAsync(msg.Topic, msg.Payload);
                            msg.Dispatched = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error dispatching OutboxMessage {Id} to {Topic}", msg.Id, msg.Topic);
                        }
                    }

                    await db.SaveChangesAsync(ct);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in OutboxDispatcher loop");
                }

                await Task.Delay(1000, ct);
            }
        }
    }
}

