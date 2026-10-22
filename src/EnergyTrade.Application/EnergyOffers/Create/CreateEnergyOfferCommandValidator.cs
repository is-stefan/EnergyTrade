using FluentValidation;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed class CreateEnergyOfferCommandValidator
    : AbstractValidator<CreateEnergyOfferCommand>
{
    public CreateEnergyOfferCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.PortfolioId)
            .NotEmpty();

        RuleFor(command => command.QuantityMWh)
            .GreaterThan(0);

        RuleFor(command => command.PricePerMWh)
            .GreaterThan(0);

        RuleFor(command => command.DeliveryStart)
            .LessThan(command => command.DeliveryEnd);
    }
}