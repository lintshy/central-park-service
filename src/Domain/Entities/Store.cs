namespace CentralPark.Domain.Entities;

public sealed class Store : BaseEntity
{
    public Guid BusinessOwnerProfileId { get; private set; }
    public Guid SuburbId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string StreetAddress { get; private set; } = string.Empty;
    public string PostCode { get; private set; } = string.Empty;
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public bool IsAcceptingOrders { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string OwnerDisplayName { get; private set; } = string.Empty;
    public string? OwnerContactEmail { get; private set; }
    public string? OwnerContactPhone { get; private set; }

    public Suburb? Suburb { get; private set; }
    public IReadOnlyList<StoreHours> Hours => _hours.AsReadOnly();
    private readonly List<StoreHours> _hours = [];

    private Store() { }

    public static Store Create(
        Guid businessOwnerProfileId,
        Guid suburbId,
        string name,
        string streetAddress,
        string postCode,
        string ownerDisplayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(streetAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerDisplayName);
        return new Store
        {
            BusinessOwnerProfileId = businessOwnerProfileId,
            SuburbId = suburbId,
            Name = name,
            StreetAddress = streetAddress,
            PostCode = postCode,
            OwnerDisplayName = ownerDisplayName
        };
    }

    public void SetAcceptingOrders(bool accepting)
    {
        IsAcceptingOrders = accepting;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        IsAcceptingOrders = false;
        SetUpdatedAt();
    }
}
