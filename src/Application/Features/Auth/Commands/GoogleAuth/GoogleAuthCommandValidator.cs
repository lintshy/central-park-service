using FluentValidation;

namespace CentralPark.Application.Features.Auth.Commands.GoogleAuth;

public sealed class GoogleAuthCommandValidator : AbstractValidator<GoogleAuthCommand>
{
    public GoogleAuthCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}