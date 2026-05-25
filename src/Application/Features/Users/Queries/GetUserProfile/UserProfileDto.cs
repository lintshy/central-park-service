namespace CentralPark.Application.Features.Users.Queries.GetUserProfile;

public sealed record UserProfileDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    SuburbDto? HomeSuburb,
    IReadOnlyList<FavouriteStoreDto> FavouriteStores,
    IReadOnlyList<RecentOrderDto> RecentOrders);

public sealed record SuburbDto(Guid Id, string Name, string PostCode, string State, string? GeoBoundaries);

public sealed record FavouriteStoreDto(Guid StoreId, string Name, string SuburbName, bool IsAcceptingOrders);

public sealed record RecentOrderDto(
    Guid OrderId,
    string StoreName,
    string Status,
    decimal TotalAmount,
    int ItemCount,
    DateTime PlacedAt);
