namespace SharedContacts.Events;

public sealed class OrderCreatedEvent
{
    public Guid EventId   { get; set; }
    public Guid OrderId   { get; set; }
    public Guid UserId    { get; set; }
    public decimal Amount { get; set; }
    
    public DateTime OccurredOn { get; set; }
}
