using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Auth.DTOs;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Auth.Commands.GoogleAuth;

public sealed class GoogleAuthCommandHandler(
    IGoogleAuthService googleAuthService,
    IIdentityService identityService,
    IJwtService jwtService)
    : IRequestHandler<GoogleAuthCommand, Result<AuthTokenDto>>
{
    private static readonly Error InvalidToken =
        new("Auth.GoogleTokenInvalid", "The Google ID token is invalid or expired.");

    public async Task<Result<AuthTokenDto>> Handle(GoogleAuthCommand cmd, CancellationToken ct)
    {
        var googleUser = await googleAuthService.VerifyIdTokenAsync(cmd.IdToken, ct);
        if (googleUser is null)
            return Result<AuthTokenDto>.Failure(InvalidToken);

        var userIdResult = await identityService.FindUserIdByEmailAsync(googleUser.Email, ct);

        Guid userId;
        if (userIdResult.IsSuccess)
        {
            userId = userIdResult.Value!;
        }
        else
        {
            var registerResult = await identityService.RegisterExternalAsync(googleUser.Email, ct);
            if (!registerResult.IsSuccess)
                return Result<AuthTokenDto>.Failure(registerResult.Error);
            userId = registerResult.Value!;
        }

        var roles = await identityService.GetRolesAsync(userId, ct);
        var accessToken = jwtService.GenerateAccessToken(userId, googleUser.Email, roles);
        var refreshToken = jwtService.GenerateRefreshToken();

        await identityService.SetRefreshTokenAsync(userId, refreshToken, DateTime.UtcNow.AddDays(7), ct);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(accessToken, refreshToken, googleUser.Email));
    }
}