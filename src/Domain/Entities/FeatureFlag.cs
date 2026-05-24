namespace CentralPark.Domain.Entities;

public sealed class FeatureFlag : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public string? Description { get; private set; }
    public Dictionary<string, bool> RolloutOverrides { get; private set; } = [];

    private FeatureFlag() { }

    public static FeatureFlag Create(string key, bool isEnabled, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return new FeatureFlag
        {
            Key = key.ToLowerInvariant(),
            IsEnabled = isEnabled,
            Description = description,
            RolloutOverrides = []
        };
    }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        SetUpdatedAt();
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        SetUpdatedAt();
    }

    public void SetRolloutOverride(string userId, bool isEnabled)
    {
        RolloutOverrides[userId] = isEnabled;
        SetUpdatedAt();
    }
}
