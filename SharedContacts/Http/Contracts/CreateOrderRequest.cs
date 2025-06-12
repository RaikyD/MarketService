namespace SharedContacts.Http.Contracts;

public class CreateOrderRequest
{
    public Guid UserId  { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}