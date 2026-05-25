using FluentValidation;

namespace CentralPark.Application.Features.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Quantity).GreaterThanOrEqualTo(1);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
