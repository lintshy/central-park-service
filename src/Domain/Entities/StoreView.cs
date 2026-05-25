namespace CentralPark.Domain.Entities;

public sealed class StoreView : BaseEntity
{
    public Guid StoreId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Source { get; private set; }

    private StoreView() { }

    public static StoreView Create(Guid storeId, Guid userId, string? source = null) =>
        new() { StoreId = storeId, UserId = userId, Source = source };
}
