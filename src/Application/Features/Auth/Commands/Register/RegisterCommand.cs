using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password, string ConfirmPassword) : ICommand<Guid>;
