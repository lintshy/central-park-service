using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Auth.DTOs;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Auth.Commands.GoogleAuthMock;

public sealed class GoogleAuthMockCommandHandler(
    IIdentityService identityService,
    IJwtService jwtService)
    : IRequestHandler<GoogleAuthMockCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(GoogleAuthMockCommand cmd, CancellationToken ct)
    {
        var userIdResult = await identityService.FindUserIdByEmailAsync(cmd.Email, ct);

        Guid userId;
        if (userIdResult.IsSuccess)
        {
            userId = userIdResult.Value!;
        }
        else
        {
            var registerResult = await identityService.RegisterExternalAsync(cmd.Email, ct);
            if (!registerResult.IsSuccess)
                return Result<AuthTokenDto>.Failure(registerResult.Error);
            userId = registerResult.Value!;
        }

        var roles = await identityService.GetRolesAsync(userId, ct);
        var accessToken = jwtService.GenerateAccessToken(userId, cmd.Email, roles);
        var refreshToken = jwtService.GenerateRefreshToken();

        await identityService.SetRefreshTokenAsync(userId, refreshToken, DateTime.UtcNow.AddDays(7), ct);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(accessToken, refreshToken, cmd.Email));
    }
}
