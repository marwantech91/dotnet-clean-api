using Domain.Entities;
using Xunit;

namespace Tests.Domain;

public class OrderTests
{
    private Order CreateOrder()
    {
        return new Order(Guid.NewGuid(), "123 Main St");
    }

    [Fact]
    public void NewOrder_HasPendingStatus()
    {
        var order = CreateOrder();
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void AddItem_IncreasesTotalAmount()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 3);

        Assert.Equal(30.00m, order.TotalAmount);
        Assert.Single(order.Items);
    }

    [Fact]
    public void AddItem_MultipleItems_SumsCorrectly()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 2);
        order.AddItem(Guid.NewGuid(), "Gadget", 25.00m, 1);

        Assert.Equal(45.00m, order.TotalAmount);
        Assert.Equal(2, order.Items.Count);
    }

    [Fact]
    public void AddItem_ZeroQuantity_Throws()
    {
        var order = CreateOrder();
        Assert.Throws<ArgumentException>(() =>
            order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 0));
    }

    [Fact]
    public void Confirm_EmptyOrder_Throws()
    {
        var order = CreateOrder();
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void Confirm_WithItems_ChangesStatus()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 1);
        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void AddItem_AfterConfirm_Throws()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 1);
        order.Confirm();

        Assert.Throws<InvalidOperationException>(() =>
            order.AddItem(Guid.NewGuid(), "Another", 5.00m, 1));
    }

    [Fact]
    public void StatusWorkflow_PendingToDelivered()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 1);

        order.Confirm();
        Assert.Equal(OrderStatus.Confirmed, order.Status);

        order.MarkProcessing();
        Assert.Equal(OrderStatus.Processing, order.Status);

        order.Ship("TRACK-123");
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal("TRACK-123", order.TrackingNumber);

        order.Deliver();
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Cancel_PendingOrder_Succeeds()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 1);
        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_ShippedOrder_Throws()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 1);
        order.Confirm();
        order.MarkProcessing();
        order.Ship("TRACK-123");

        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void Ship_WithoutProcessing_Throws()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Widget", 10.00m, 1);
        order.Confirm();

        Assert.Throws<InvalidOperationException>(() => order.Ship("TRACK-123"));
    }
}

public class OrderItemTests
{
    [Fact]
    public void TotalPrice_CalculatesCorrectly()
    {
        var item = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", 15.50m, 4);
        Assert.Equal(62.00m, item.TotalPrice);
    }

    [Fact]
    public void UpdateQuantity_ZeroThrows()
    {
        var item = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", 10.00m, 1);
        Assert.Throws<ArgumentException>(() => item.UpdateQuantity(0));
    }

    [Fact]
    public void UpdateQuantity_UpdatesCorrectly()
    {
        var item = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), "Widget", 10.00m, 1);
        item.UpdateQuantity(5);
        Assert.Equal(5, item.Quantity);
        Assert.Equal(50.00m, item.TotalPrice);
    }
}
