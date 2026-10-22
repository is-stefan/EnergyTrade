using FluentValidation;

namespace EnergyTrade.Application.Orders.Create;

public sealed class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.PortfolioId)
            .NotEmpty();

        RuleFor(command => command.QuantityMWh)
            .GreaterThan(0);

        RuleFor(command => command.MaxPricePerMWh)
            .GreaterThan(0);

        RuleFor(command => command.DeliveryStart)
            .LessThan(command => command.DeliveryEnd);
    }
}