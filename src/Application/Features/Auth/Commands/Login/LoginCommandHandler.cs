using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Auth.DTOs;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(IIdentityService identityService, IJwtService jwtService)
    : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var result = await identityService.SignInAsync(cmd.Email, cmd.Password, ct);
        if (!result.IsSuccess)
            return Result<AuthTokenDto>.Failure(result.Error);

        var user = result.Value!;
        var roles = await identityService.GetRolesAsync(user.UserId, ct);
        var accessToken = jwtService.GenerateAccessToken(user.UserId, user.Email, roles);
        var refreshToken = jwtService.GenerateRefreshToken();

        await identityService.SetRefreshTokenAsync(user.UserId, refreshToken,
            DateTime.UtcNow.AddDays(7), ct);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(accessToken, refreshToken, user.Email));
    }
}
