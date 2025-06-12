using System.Text.Json;
using Confluent.Kafka;

namespace OrdersService.Infrastructure.Kafka;

public class KafkaProducer : IDisposable
{
    private readonly IProducer<Null,string> _producer;
    private readonly ILogger<KafkaProducer> _log;

    public KafkaProducer(ProducerConfig cfg, ILogger<KafkaProducer> log)
    {
        _producer = new ProducerBuilder<Null,string>(cfg).Build();
        _log      = log;
    }

    public async Task ProduceAsync(string topic, object @event)
    {
        // если event уже строка — отправляем её как есть
        string payload = @event switch
        {
            string s => s,
            _        => JsonSerializer.Serialize(@event, @event.GetType())
        };

        try
        {
            var dr = await _producer.ProduceAsync(
                topic,
                new Message<Null,string> { Value = payload }
            );
            _log.LogDebug("Sent to {Topic} [part:{Partition} off:{Offset}]",
                topic, dr.Partition, dr.Offset);
        }
        catch (ProduceException<Null,string> ex)
        {
            _log.LogError(ex, "Failed to publish to {Topic}: {Payload}", topic, payload);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}