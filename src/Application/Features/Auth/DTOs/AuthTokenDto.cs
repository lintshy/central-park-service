namespace CentralPark.Application.Features.Auth.DTOs;

public sealed record AuthTokenDto(string AccessToken, string RefreshToken, string Email);
