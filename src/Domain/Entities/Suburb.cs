namespace CentralPark.Domain.Entities;

public sealed class Suburb : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string PostCode { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Country { get; private set; } = "AU";
    public decimal? CentroidLat { get; private set; }
    public decimal? CentroidLng { get; private set; }
    public string? GeoBoundaries { get; private set; }

    private Suburb() { }

    public static Suburb Create(string name, string postCode, string state, string country = "AU")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(postCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        return new Suburb { Name = name, PostCode = postCode, State = state, Country = country };
    }

    public void SetCoordinates(decimal lat, decimal lng)
    {
        CentroidLat = lat;
        CentroidLng = lng;
        SetUpdatedAt();
    }
}
