using SharedContacts.Http.Contracts;

namespace SharedContacts.Http.Clients.Interfaces;

public interface IPaymentsClient
{
    Task<decimal> GetBalanceAsync(Guid userId);
    Task TopUpAsync(Guid userId, AdjustBalanceRequest dto);
    Task WithdrawAsync(Guid userId, AdjustBalanceRequest dto);
}