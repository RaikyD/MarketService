using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PaymentsService.Application.Services;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.DbData;

namespace PaymentServiceTests;
public class PaymentServiceTests
{
    private static PaymentDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w => 
                w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task CreateUserBalance_ShouldAddNewAccount()
    {
        // Arrange
        var db     = CreateContext(nameof(CreateUserBalance_ShouldAddNewAccount));
        var svc    = new PaymentService(db, null!);
        var userId = Guid.NewGuid();

        // Act
        var returned = await svc.CreateUserBalance(userId, 123.45m);

        // Assert
        Assert.Equal(userId, returned);
        var saved = await db.Users.FindAsync(userId);
        Assert.NotNull(saved);
        Assert.Equal(123.45m, saved!.Balance);
    }

    [Fact]
    public async Task CreateUserBalance_Duplicate_ThrowsInvalidOperationException()
    {
        // Arrange
        var db     = CreateContext(
                        nameof(CreateUserBalance_Duplicate_ThrowsInvalidOperationException));
        var svc    = new PaymentService(db, null!);
        var userId = Guid.NewGuid();
        await svc.CreateUserBalance(userId, 0m);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.CreateUserBalance(userId, 50m)
        );
    }

    [Fact]
    public async Task GetUserBalance_Existing_ReturnsBalance()
    {
        // Arrange
        var db     = CreateContext(nameof(GetUserBalance_Existing_ReturnsBalance));
        var userId = Guid.NewGuid();
        db.Users.Add(new UserAccount(userId, 777m));
        await db.SaveChangesAsync();
        var svc = new PaymentService(db, null!);

        // Act
        var balance = await svc.GetUserBalance(userId);

        // Assert
        Assert.Equal(777m, balance);
    }

    [Fact]
    public async Task GetUserBalance_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var db  = CreateContext(
                    nameof(GetUserBalance_NonExisting_ThrowsKeyNotFoundException));
        var svc = new PaymentService(db, null!);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => svc.GetUserBalance(Guid.NewGuid())
        );
    }

    [Fact]
    public async Task TopUpUserBalance_Valid_IncrementsBalance()
    {
        // Arrange
        var db     = CreateContext(nameof(TopUpUserBalance_Valid_IncrementsBalance));
        var userId = Guid.NewGuid();
        db.Users.Add(new UserAccount(userId, 100m));
        await db.SaveChangesAsync();
        var svc = new PaymentService(db, null!);

        // Act
        var newBal = await svc.TopUpUserBalance(userId, 50m);

        // Assert
        Assert.Equal(150m, newBal);
        var saved = await db.Users.FindAsync(userId);
        Assert.Equal(150m, saved!.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task TopUpUserBalance_NonPositive_ThrowsArgumentException(decimal badAmount)
    {
        // Arrange
        var db     = CreateContext(
                        nameof(TopUpUserBalance_NonPositive_ThrowsArgumentException));
        var userId = Guid.NewGuid();
        db.Users.Add(new UserAccount(userId, 0m));
        await db.SaveChangesAsync();
        var svc = new PaymentService(db, null!);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => svc.TopUpUserBalance(userId, badAmount)
        );
    }

    [Fact]
    public async Task TopUpUserBalance_NonExistingUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var db  = CreateContext(
                    nameof(TopUpUserBalance_NonExistingUser_ThrowsKeyNotFoundException));
        var svc = new PaymentService(db, null!);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => svc.TopUpUserBalance(Guid.NewGuid(), 10m)
        );
    }
}

