namespace OrderServiceTests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrdersService.Application.Services;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.DbData;
using OrdersService.Infrastructure.Repositories;
using Xunit;


public class OrderServiceTests
{
    private static OrderDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new OrderDbContext(options);
    }

    [Fact]
    public async Task AddOrderAsync_ShouldPersistOrder()
    {
        // Arrange
        var db      = CreateContext(nameof(AddOrderAsync_ShouldPersistOrder));
        var repo    = new OrderRepository(db);
        var service = new OrderService(repo);
        var userId  = Guid.NewGuid();
        decimal amount = 42.5m;
        string desc    = "Test order";

        // Act
        var newId = await service.AddOrderAsync(userId, amount, desc);

        // Assert
        var saved = await db.Orders.FindAsync(newId);
        Assert.NotNull(saved);
        Assert.Equal(userId,        saved!.UserId);
        Assert.Equal(amount,        saved.Amount);
        Assert.Equal(desc,          saved.Description);
        Assert.Equal(StatusType.New, saved.Status);
    }

    [Fact]
    public async Task GetOrderAsync_Existing_ReturnsOrder()
    {
        // Arrange
        var db      = CreateContext(nameof(GetOrderAsync_Existing_ReturnsOrder));
        var order   = new Order(Guid.NewGuid(), 10m, "Hello");
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var repo    = new OrderRepository(db);
        var service = new OrderService(repo);

        // Act
        var fetched = await service.GetOrderAsync(order.Id);

        // Assert
        Assert.Equal(order.Id,          fetched.Id);
        Assert.Equal(order.UserId,      fetched.UserId);
        Assert.Equal(order.Amount,      fetched.Amount);
        Assert.Equal(order.Description, fetched.Description);
        Assert.Equal(order.Status,      fetched.Status);
    }

    [Fact]
    public async Task GetOrderAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var db      = CreateContext(nameof(GetOrderAsync_NonExisting_ThrowsKeyNotFoundException));
        var repo    = new OrderRepository(db);
        var service = new OrderService(repo);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetOrderAsync(Guid.NewGuid())
        );
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnAll()
    {
        // Arrange
        var db      = CreateContext(nameof(GetAllOrdersAsync_ShouldReturnAll));
        var o1      = new Order(Guid.NewGuid(), 1m, "A");
        var o2      = new Order(Guid.NewGuid(), 2m, "B");
        db.Orders.AddRange(o1, o2);
        await db.SaveChangesAsync();
        var repo    = new OrderRepository(db);
        var service = new OrderService(repo);

        // Act
        var list = await service.GetAllOrdersAsync();

        // Assert
        Assert.IsType<List<Order>>(list);
        Assert.Equal(2, list.Count);
    }
}

