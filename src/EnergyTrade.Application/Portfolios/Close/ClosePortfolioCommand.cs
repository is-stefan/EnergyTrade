using MediatR;

namespace EnergyTrade.Application.Portfolios.Close;

public sealed record ClosePortfolioCommand(
    Guid UserId,
    Guid PortfolioId)
    : IRequest<bool>;
