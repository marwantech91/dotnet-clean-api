namespace Domain.Entities;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public class Order : Entity
{
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? TrackingNumber { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    private Order() { }

    public Order(Guid userId, string shippingAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ShippingAddress = shippingAddress;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify a non-pending order");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        var item = new OrderItem(Id, productId, productName, unitPrice, quantity);
        Items.Add(item);
        RecalculateTotal();
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify a non-pending order");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item not found");

        Items.Remove(item);
        RecalculateTotal();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (Items.Count == 0)
            throw new InvalidOperationException("Cannot confirm an empty order");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkProcessing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be processed");

        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered");

        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel this order");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.TotalPrice);
        UpdatedAt = DateTime.UtcNow;
    }
}

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
