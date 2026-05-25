namespace CentralPark.Domain.Entities;

public sealed class CustomerProfile : BaseEntity
{
    public Guid UserId { get; private set; }

    public IReadOnlyList<Favourite> Favourites => _favourites.AsReadOnly();
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

    private readonly List<Favourite> _favourites = [];
    private readonly List<Order> _orders = [];

    private CustomerProfile() { }

    public static CustomerProfile Create(Guid userId) =>
        new() { UserId = userId };
}
