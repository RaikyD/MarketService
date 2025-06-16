namespace PaymentsService.Domain.Entities;

using System;


public sealed class InboxMessage
{
    public Guid EventId { get; set; }           // уникальный ID пришедшего события
    public string Topic { get; set; } = null!;
    public DateTime ReceivedOn { get; set; }    // когда впервые принято
}

