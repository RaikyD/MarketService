using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Interfaces;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.DbData;
using PaymentsService.Infrastructure.Kafka;

namespace PaymentsService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _db;
    private readonly KafkaProducer   _producer;

    public PaymentService(PaymentDbContext db, KafkaProducer producer)
    {
        _db       = db;
        _producer = producer;
    }

    public async Task<decimal> GetUserBalance(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException();
        return user.Balance;
    }

    public async Task<decimal> TopUpUserBalance(Guid userId, decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        
        await using var tx = await _db.Database.BeginTransactionAsync();
        
        var user = await _db.Users.FindAsync(userId)
                   ?? throw new KeyNotFoundException();
        user.TopUp(amount);
        _db.Users.Update(user);
        
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return user.Balance;
    }

    public async Task<Guid> CreateUserBalance(Guid userId, decimal? startBalance)
    {
        if (await _db.Users.FindAsync(userId) != null)
            throw new InvalidOperationException("Account already exists");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var initial = startBalance.GetValueOrDefault(0);
        var account = new UserAccount(userId, initial);
        _db.Users.Add(account);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return userId;
    }
}
