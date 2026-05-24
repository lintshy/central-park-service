using CentralPark.Application.Common.Markers;
using CentralPark.Application.Features.Auth.DTOs;

namespace CentralPark.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthTokenDto>;
