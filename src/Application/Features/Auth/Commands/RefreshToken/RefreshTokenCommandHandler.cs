using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Auth.DTOs;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(IJwtService jwtService, IIdentityService identityService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    private static readonly Error InvalidToken = new("Auth.InvalidToken", "The provided token is invalid or expired.");

    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(cmd.AccessToken);
        if (principal is null)
            return Result<AuthTokenDto>.Failure(InvalidToken);

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result<AuthTokenDto>.Failure(InvalidToken);

        var valid = await identityService.ValidateRefreshTokenAsync(userId, cmd.RefreshToken, ct);
        if (!valid)
            return Result<AuthTokenDto>.Failure(InvalidToken);

        var userResult = await identityService.GetByIdAsync(userId, ct);
        if (!userResult.IsSuccess)
            return Result<AuthTokenDto>.Failure(InvalidToken);

        var user = userResult.Value!;
        var roles = await identityService.GetRolesAsync(userId, ct);
        var newAccess = jwtService.GenerateAccessToken(userId, user.Email, roles);
        var newRefresh = jwtService.GenerateRefreshToken();
        await identityService.SetRefreshTokenAsync(userId, newRefresh, DateTime.UtcNow.AddDays(7), ct);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(newAccess, newRefresh, user.Email));
    }
}
