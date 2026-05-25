using CentralPark.Domain.Enums;

namespace CentralPark.Domain.Entities;

public sealed class Order : BaseEntity
{
    public Guid CustomerProfileId { get; private set; }
    public Guid StoreId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }

    public Store? Store { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    private readonly List<OrderItem> _items = [];

    private Order() { }

    public static Order Create(Guid customerProfileId, Guid storeId, string? notes = null) =>
        new()
        {
            CustomerProfileId = customerProfileId,
            StoreId = storeId,
            Status = OrderStatus.Pending,
            Notes = notes
        };

    public void AddItem(string productName, int quantity, decimal unitPrice)
    {
        var item = OrderItem.Create(Id, productName, quantity, unitPrice);
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.LineTotal);
        SetUpdatedAt();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        SetUpdatedAt();
    }
}
