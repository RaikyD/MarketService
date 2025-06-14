namespace SharedContacts.Http.Contracts;

public enum StatusType
{
    New,
    Finished,
    Canceled
}

public class OrderDto
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    
    public string Description { get; private set; }
    
    public StatusType Status { get; private set; }
    
}