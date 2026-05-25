using CentralPark.Application.Common.Markers;
using CentralPark.Application.Features.Auth.DTOs;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Auth.Commands.GoogleAuth;

public sealed record GoogleAuthCommand(string IdToken) : ICommand<AuthTokenDto>;