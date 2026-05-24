using CentralPark.Application.Features.Auth.DTOs;
using CentralPark.Shared;

namespace CentralPark.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<UserDto>> SignInAsync(string email, string password, CancellationToken ct = default);
    Task<Result<Guid>> RegisterAsync(string email, string password, CancellationToken ct = default);
    Task<Result<UserDto>> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct = default);
    Task SetRefreshTokenAsync(Guid userId, string token, DateTime expiry, CancellationToken ct = default);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);
}
