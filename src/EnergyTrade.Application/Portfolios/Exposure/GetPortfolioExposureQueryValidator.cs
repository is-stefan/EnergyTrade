using FluentValidation;

namespace EnergyTrade.Application.Portfolios.Exposure;

public sealed class GetPortfolioExposureQueryValidator
    : AbstractValidator<GetPortfolioExposureQuery>
{
    public GetPortfolioExposureQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty();

        RuleFor(query => query.PortfolioId)
            .NotEmpty();
    }
}