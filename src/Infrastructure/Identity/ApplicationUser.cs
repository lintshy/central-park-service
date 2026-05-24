using Microsoft.AspNetCore.Identity;

namespace CentralPark.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }

    public bool IsRefreshTokenValid(string token) =>
        RefreshToken == token && RefreshTokenExpiry > DateTime.UtcNow;
}
