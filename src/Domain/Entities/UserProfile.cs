namespace CentralPark.Domain.Entities;

public sealed class UserProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid? HomeSuburbId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }

    public Suburb? HomeSuburb { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(Guid userId, string firstName, string lastName, string? avatarUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        return new UserProfile
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            AvatarUrl = avatarUrl
        };
    }

    public void UpdateHomeSuburb(Guid suburbId)
    {
        HomeSuburbId = suburbId;
        SetUpdatedAt();
    }

    public void UpdateContact(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
        SetUpdatedAt();
    }
}
