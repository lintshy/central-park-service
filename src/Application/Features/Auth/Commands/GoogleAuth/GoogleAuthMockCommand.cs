using CentralPark.Application.Common.Markers;
using CentralPark.Application.Features.Auth.DTOs;

namespace CentralPark.Application.Features.Auth.Commands.GoogleAuthMock;

public sealed record GoogleAuthMockCommand(string Email) : ICommand<AuthTokenDto>;