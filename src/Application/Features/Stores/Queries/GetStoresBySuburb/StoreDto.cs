namespace CentralPark.Application.Features.Stores.Queries.GetStoresBySuburb;

public sealed record StoreDto(
    Guid StoreId,
    string Name,
    string? Description,
    string StreetAddress,
    string PostCode,
    bool IsAcceptingOrders,
    string OwnerDisplayName,
    string? OwnerContactPhone,
    IReadOnlyList<StoreHoursDto> OpenHours);

public sealed record StoreHoursDto(int DayOfWeek, string? OpenTime, string? CloseTime, bool IsClosed);
