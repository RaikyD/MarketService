namespace OrdersService.Domain.Entities;


public class InboxMessage
{
    public Guid   EventId    { get; set; }    
    public string Topic      { get; set; }    
    public DateTime ReceivedOn { get; set; }  
}

