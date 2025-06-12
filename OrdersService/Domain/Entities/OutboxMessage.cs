namespace OrdersService.Domain.Entities;

public class OutboxMessage
{
    public Guid   Id         { get; set; }   
    public string Topic      { get; set; }   
    public string Payload    { get; set; }   
    public DateTime OccurredOn { get; set; } 
    public bool   Dispatched { get; set; }   
}

