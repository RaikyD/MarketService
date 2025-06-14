using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentsService.Infrastructure.Kafka
{

    public class KafkaConsumer<T> : BackgroundService where T : class
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _topic;
        private readonly Func<T, Task> _handler;
        private readonly ILogger<KafkaConsumer<T>> _logger;

        public KafkaConsumer(ConsumerConfig cfg, string topic, Func<T, Task> handler, ILogger<KafkaConsumer<T>> logger)
        {
            _consumer  = new ConsumerBuilder<Ignore, string>(cfg).Build();
            _topic     = topic;
            _handler   = handler;
            _logger    = logger;
        }

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            _consumer.Subscribe(_topic);
            return Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var res = _consumer.Consume(ct);
                        var obj = JsonSerializer.Deserialize<T>(res.Message.Value);
                        if (obj is not null)
                        {
                            await _handler(obj);
                        }
                        _consumer.Commit(res);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Deserialization failed on topic {Topic}", _topic);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in KafkaConsumer<{Type}> loop", typeof(T).Name);
                    }
                }
            }, ct);
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
