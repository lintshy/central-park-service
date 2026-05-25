namespace CentralPark.Domain.Entities;

public sealed class BusinessOwnerProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string BusinessName { get; private set; } = string.Empty;
    public string? BusinessEmail { get; private set; }
    public string? BusinessPhone { get; private set; }

    public IReadOnlyList<Store> Stores => _stores.AsReadOnly();
    private readonly List<Store> _stores = [];

    private BusinessOwnerProfile() { }

    public static BusinessOwnerProfile Create(Guid userId, string businessName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);
        return new BusinessOwnerProfile { UserId = userId, BusinessName = businessName };
    }

    public void UpdateContact(string? email, string? phone)
    {
        BusinessEmail = email;
        BusinessPhone = phone;
        SetUpdatedAt();
    }
}
