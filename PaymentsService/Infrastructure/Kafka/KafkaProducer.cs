// using Confluent.Kafka;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Text.Json;
// using System.Threading.Tasks;
//
// namespace PaymentsService.Infrastructure.Kafka
// {
//     /// <summary>
//     /// Обёртка над Confluent.Kafka Producer.
//     /// </summary>
//     public class KafkaProducer : IDisposable
//     {
//         private readonly IProducer<Null, string> _producer;
//         private readonly ILogger<KafkaProducer> _logger;
//
//         public KafkaProducer(ProducerConfig cfg, ILogger<KafkaProducer> logger)
//         {
//             _producer = new ProducerBuilder<Null, string>(cfg).Build();
//             _logger   = logger;
//         }
//
//         public async Task ProduceAsync(string topic, object @event)
//         {
//             var json = JsonSerializer.Serialize(@event, @event.GetType());
//             try
//             {
//                 var dr = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = json });
//                 _logger.LogDebug("Sent event to {Topic} [partition:{Partition} | offset:{Offset}]", 
//                     topic, dr.Partition, dr.Offset);
//             }
//             catch (ProduceException<Null, string> ex)
//             {
//                 _logger.LogError(ex, "Failed to publish to topic {Topic}: {Payload}", topic, json);
//                 throw;
//             }
//         }
//
//         public void Dispose()
//         {
//             _producer.Flush(TimeSpan.FromSeconds(5));
//             _producer.Dispose();
//         }
//     }
// }
//

using System.Text.Json;
using Confluent.Kafka;

namespace PaymentsService.Infrastructure.Kafka;

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