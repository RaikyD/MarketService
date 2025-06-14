namespace PaymentServiceTests;

using System;
using PaymentsService.Domain.Entities;
using Xunit;


public class UserAccountTests
{
    [Fact]
    public void TopUp_Positive_AddsToBalance()
    {
        // Arrange
        var account = new UserAccount(Guid.NewGuid(), 100m);

        // Act
        account.TopUp(50m);

        // Assert
        Assert.Equal(150m, account.Balance);
    }

    [Fact]
    public void TopUp_Negative_ThrowsArgumentException()
    {
        // Arrange
        var account = new UserAccount(Guid.NewGuid(), 100m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => account.TopUp(-1m));
    }

    [Fact]
    public void Withdraw_Positive_SubtractsFromBalance()
    {
        // Arrange
        var account = new UserAccount(Guid.NewGuid(), 100m);

        // Act
        account.Withdraw(30m);

        // Assert
        Assert.Equal(70m, account.Balance);
    }

    [Fact]
    public void Withdraw_Negative_ThrowsArgumentException()
    {
        // Arrange
        var account = new UserAccount(Guid.NewGuid(), 100m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => account.Withdraw(-5m));
    }

    [Fact]
    public void Withdraw_TooMuch_ThrowsArgumentException()
    {
        // Arrange
        var account = new UserAccount(Guid.NewGuid(), 100m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => account.Withdraw(150m));
    }
}

