using FluentValidation;

namespace CentralPark.Application.Features.Auth.Commands.GoogleAuthMock;

public sealed class GoogleAuthMockCommandValidator : AbstractValidator<GoogleAuthMockCommand>
{
    public GoogleAuthMockCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}