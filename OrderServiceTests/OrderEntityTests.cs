namespace OrderServiceTests;

using System;
using OrdersService.Domain.Entities;
using Xunit;


public class OrderEntityTests
{
    [Fact]
    public void NewOrder_DefaultStatusIsNew()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), 5m, "X");

        // Act
        var status = order.Status;

        // Assert
        Assert.Equal(StatusType.New, status);
    }

    [Fact]
    public void MarkFinished_FromNew_SetsFinished()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), 5m, "X");

        // Act
        order.MarkFinished();

        // Assert
        Assert.Equal(StatusType.Finished, order.Status);
    }

    [Fact]
    public void MarkFinished_NotNew_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), 5m, "X");
        order.MarkFinished();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.MarkFinished());
    }

    [Fact]
    public void MarkCanceled_FromNew_SetsCanceled()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), 5m, "X");

        // Act
        order.MarkCanceled();

        // Assert
        Assert.Equal(StatusType.Canceled, order.Status);
    }

    [Fact]
    public void MarkCanceled_NotNew_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), 5m, "X");
        order.MarkCanceled();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.MarkCanceled());
    }
}

