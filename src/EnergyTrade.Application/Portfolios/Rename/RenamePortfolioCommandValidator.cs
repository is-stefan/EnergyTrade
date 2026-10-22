using FluentValidation;

namespace EnergyTrade.Application.Portfolios.Rename;

public sealed class RenamePortfolioCommandValidator
    : AbstractValidator<RenamePortfolioCommand>
{
    public RenamePortfolioCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.PortfolioId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty();
    }
}