using SharedContacts.Http.Contracts;

namespace SharedContacts.Events;

public sealed class PaymentCompletedEvent
{
    public Guid   EventId   { get; set; }
    public Guid   OrderId   { get; set; }
    public Guid   UserId    { get; set; }
    public decimal Amount   { get; set; }
    public DateTime OccurredOn { get; set; }
    public StatusType Status { get; set; }
}

