using CentralPark.Application.Common.Markers;
using CentralPark.Application.Features.Auth.DTOs;

namespace CentralPark.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<AuthTokenDto>;
