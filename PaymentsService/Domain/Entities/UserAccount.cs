using System.Data.Common;

namespace PaymentsService.Domain.Entities;

public class UserAccount
{
    public Guid UsedId { get; }
    public decimal Balance { get; private set; }
    
    private UserAccount() {}
    
    public UserAccount(Guid usedId, decimal balance)
    {
        UsedId = usedId;
        Balance = balance;
    }

    public void TopUp(decimal sum)
    {
        if (sum < 0) throw new ArgumentException("Amount must be positive");

        Balance += sum;
    }

    public void Withdraw(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount must be positive");
        if (Balance - amount < 0) throw new ArgumentException("Not enough money for this operation");

        Balance -= amount;
    }
}