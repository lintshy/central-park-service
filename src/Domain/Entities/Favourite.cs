namespace CentralPark.Domain.Entities;

public sealed class Favourite : BaseEntity
{
    public Guid CustomerProfileId { get; private set; }
    public Guid StoreId { get; private set; }

    public Store? Store { get; private set; }

    private Favourite() { }

    public static Favourite Create(Guid customerProfileId, Guid storeId) =>
        new() { CustomerProfileId = customerProfileId, StoreId = storeId };
}
