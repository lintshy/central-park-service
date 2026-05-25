namespace CentralPark.Domain.Entities;

public sealed class StoreHours : BaseEntity
{
    public Guid StoreId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly? OpenTime { get; private set; }
    public TimeOnly? CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    private StoreHours() { }

    public static StoreHours CreateOpen(Guid storeId, DayOfWeek day, TimeOnly open, TimeOnly close) =>
        new() { StoreId = storeId, DayOfWeek = day, OpenTime = open, CloseTime = close, IsClosed = false };

    public static StoreHours CreateClosed(Guid storeId, DayOfWeek day) =>
        new() { StoreId = storeId, DayOfWeek = day, IsClosed = true };
}
