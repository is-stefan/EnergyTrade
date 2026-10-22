using FluentValidation;

namespace EnergyTrade.Application.EnergyOffers.Update;

public sealed class UpdateEnergyOfferCommandValidator
    : AbstractValidator<UpdateEnergyOfferCommand>
{
    public UpdateEnergyOfferCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.EnergyOfferId)
            .NotEmpty();

        RuleFor(command => command.QuantityMWh)
            .GreaterThan(0);

        RuleFor(command => command.PricePerMWh)
            .GreaterThan(0);

        RuleFor(command => command.DeliveryStart)
            .LessThan(command => command.DeliveryEnd);
    }
}