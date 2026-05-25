using CentralPark.Application.Common.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CentralPark.Infrastructure.Identity;

public sealed class GoogleAuthService(
    IConfiguration configuration,
    ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken, CancellationToken ct)
    {
        var clientId = configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is not configured.");

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return new GoogleUserInfo(payload.Email, payload.Subject);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning("Google ID token validation failed: {Reason}", ex.Message);
            return null;
        }
    }
}
