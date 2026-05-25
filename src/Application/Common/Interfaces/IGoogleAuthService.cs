namespace CentralPark.Application.Common.Interfaces;

public sealed record GoogleUserInfo(string Email, string GoogleId);

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
}