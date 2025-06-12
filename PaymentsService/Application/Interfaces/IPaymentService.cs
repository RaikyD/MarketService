namespace PaymentsService.Application.Interfaces;

public interface IPaymentService
{
    public Task<decimal> GetUserBalance(Guid userId);
    public Task<decimal> TopUpUserBalance(Guid userId, decimal amount);
    public Task<Guid> CreateUserBalance(Guid userId, decimal? startBalance);
}