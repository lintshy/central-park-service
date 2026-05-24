using CentralPark.Application.Common.Interfaces;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCommand cmd, CancellationToken ct)
        => await identityService.RegisterAsync(cmd.Email, cmd.Password, ct);
}
