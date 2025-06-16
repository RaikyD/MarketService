namespace PaymentsService.Domain.Entities;

using System;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }               
    public string Topic { get; set; } = null!; 
    public string Payload { get; set; } = null!; // сериализованное JSON события
    public DateTime OccurredOn { get; set; }   
    public bool Dispatched { get; set; }        // было ли отослано в Kafka
}

