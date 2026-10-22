using FluentValidation;

namespace EnergyTrade.Application.Portfolios.Create;

public sealed class CreatePortfolioCommandValidator
    : AbstractValidator<CreatePortfolioCommand>
{
    public CreatePortfolioCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty();
    }
}