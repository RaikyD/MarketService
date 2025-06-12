using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.DbData;
using PaymentsService.Infrastructure.Interfaces;

namespace PaymentsService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _db;

    public PaymentRepository(PaymentDbContext db)
    {
        _db = db;
    }
    
    public async Task<UserAccount?> GetUserByIdAsync(Guid userId) =>
        await _db.Users.FindAsync(userId);

    public async Task AddAsync(UserAccount userAccount)
    {
        await _db.Users.AddAsync(userAccount);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserAccount userAccount)
    {
        _db.Users.Update(userAccount);
        await _db.SaveChangesAsync();
    }
}