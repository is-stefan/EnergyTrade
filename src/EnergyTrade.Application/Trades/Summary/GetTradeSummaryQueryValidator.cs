using FluentValidation;

namespace EnergyTrade.Application.Trades.Summary;

public sealed class GetTradeSummaryQueryValidator
    : AbstractValidator<GetTradeSummaryQuery>
{
    public GetTradeSummaryQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty();

        RuleFor(query => query.Currency)
            .IsInEnum();

        RuleFor(query => query.EnergyType)
            .IsInEnum()
            .When(query => query.EnergyType.HasValue);
    }
}