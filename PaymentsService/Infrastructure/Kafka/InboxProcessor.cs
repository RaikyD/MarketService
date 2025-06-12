using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.DbData;
using SharedContacts.Events;

namespace PaymentsService.Infrastructure.Kafka
{
    /// <summary>
    /// Фоновый сервис, который слушает топик "order-created",
    /// кладёт в InboxMessage для идемпотентности, списывает баланс в Users
    /// и пушит в OutboxMessage событие payment-completed.
    /// </summary>
    public class InboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ConsumerConfig   _cfg;
        private readonly ILogger<InboxProcessor> _logger;

        // (п.3-4: тема пока хардкодом)
        private const string Topic = "order-created";

        public InboxProcessor(
            IServiceProvider sp,
            ConsumerConfig cfg,
            ILogger<InboxProcessor> logger)
        {
            _sp     = sp;
            _cfg    = cfg;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_cfg).Build();
            consumer.Subscribe(Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<Ignore, string> cr;
                    try
                    {
                        cr = consumer.Consume(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Consume error, retrying...");
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    OrderCreatedEvent evt;
                    try
                    {
                        evt = JsonSerializer.Deserialize<OrderCreatedEvent>(cr.Message.Value)
                              ?? throw new JsonException("Null payload");
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Bad JSON in topic {Topic}: {Payload}", Topic, cr.Message.Value);
                        consumer.Commit(cr);
                        continue;
                    }

                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                    // идемпотентность
                    if (await db.Inbox.FindAsync(evt.EventId) is not null)
                    {
                        consumer.Commit(cr);
                        continue;
                    }

                    await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);

                    // сохраняем в Inbox для идемпотентности
                    db.Inbox.Add(new InboxMessage
                    {
                        EventId    = evt.EventId,
                        Topic      = Topic,
                        ReceivedOn = DateTime.UtcNow
                    });

                    // списываем баланс
                    var user = await db.Users.FindAsync(evt.UserId)
                               ?? throw new KeyNotFoundException($"User {evt.UserId} not found");
                    user.Withdraw(evt.Amount);
                    db.Users.Update(user);

                    // готовим событие о завершении оплаты
                    var doneEvt = new PaymentCompletedEvent
                    {
                        EventId    = Guid.NewGuid(),
                        OrderId    = evt.OrderId,
                        Amount     = evt.Amount,
                        OccurredOn = DateTime.UtcNow
                    };
                    db.Outbox.Add(new OutboxMessage
                    {
                        Id         = doneEvt.EventId,
                        Topic      = "payment-completed",
                        Payload    = JsonSerializer.Serialize(doneEvt),
                        OccurredOn = doneEvt.OccurredOn,
                        Dispatched = false
                    });

                    await db.SaveChangesAsync(stoppingToken);
                    await tx.CommitAsync(stoppingToken);

                    consumer.Commit(cr);
                }
            }
            catch (OperationCanceledException) { /* shutdown */ }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in InboxProcessor, stopping host");
                throw;
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}

