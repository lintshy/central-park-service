using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Auth.DTOs;
using CentralPark.Shared;
using Microsoft.AspNetCore.Identity;

namespace CentralPark.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    private static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Email or password is incorrect.");
    private static readonly Error UserNotFound =
        new("Auth.NotFound", "User was not found.");

    public async Task<Result<UserDto>> SignInAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<UserDto>.Failure(InvalidCredentials);

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Result<UserDto>.Failure(InvalidCredentials);

        return Result<UserDto>.Success(new UserDto(user.Id, user.Email!));
    }

    public async Task<Result<Guid>> RegisterAsync(string email, string password, CancellationToken ct = default)
    {
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result<Guid>.Failure(new Error($"Auth.{error.Code}", error.Description));
        }

        return Result<Guid>.Success(user.Id);
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<UserDto>.Failure(UserNotFound);

        return Result<UserDto>.Success(new UserDto(user.Id, user.Email!));
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return [];

        var roles = await userManager.GetRolesAsync(user);
        return roles.ToList().AsReadOnly();
    }

    public async Task SetRefreshTokenAsync(Guid userId, string token, DateTime expiry, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return;

        user.SetRefreshToken(token, expiry);
        await userManager.UpdateAsync(user);
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user?.IsRefreshTokenValid(token) ?? false;
    }
}
