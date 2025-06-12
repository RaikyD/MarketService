using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Interfaces;

public interface IPaymentRepository
{
    Task<UserAccount?> GetUserByIdAsync(Guid userId);
    Task AddAsync(UserAccount userAccount);
    Task UpdateAsync(UserAccount userAccount);
}