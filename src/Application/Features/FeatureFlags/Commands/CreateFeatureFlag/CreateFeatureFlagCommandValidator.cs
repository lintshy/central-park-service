using FluentValidation;

namespace CentralPark.Application.Features.FeatureFlags.Commands.CreateFeatureFlag;

public sealed class CreateFeatureFlagCommandValidator : AbstractValidator<CreateFeatureFlagCommand>
{
    public CreateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-z0-9._\-]+$")
            .WithMessage("Key must contain only lowercase letters, digits, dots, underscores, or hyphens.");
    }
}
