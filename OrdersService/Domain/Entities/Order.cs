namespace OrdersService.Domain.Entities
{
    public enum StatusType
    {
        New,
        Finished,
        Canceled
    }

    public class Order
    {
        public Guid      Id           { get; private set; }
        public Guid      UserId       { get; private set; }
        public decimal   Amount       { get; private set; }
        public string    Description  { get; private set; }
        public StatusType Status       { get; private set; }

        private Order() {  }

        public Order(Guid userId, decimal amount, string description = "")
        {
            Id          = Guid.NewGuid();
            UserId      = userId;
            Amount      = amount;
            Description = description;
            Status      = StatusType.New;
        }

        public void MarkFinished()
        {
            if (Status != StatusType.New)
                throw new InvalidOperationException("Order can be finished only from New state.");
            Status = StatusType.Finished;
        }

        public void MarkCanceled()
        {
            if (Status != StatusType.New)
                throw new InvalidOperationException("Order can be canceled only from New state.");
            Status = StatusType.Canceled;
        }
    }
}