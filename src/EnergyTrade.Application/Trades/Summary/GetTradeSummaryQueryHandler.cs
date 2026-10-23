using EnergyTrade.Application.Abstractions.Persistence;
using MediatR;

namespace EnergyTrade.Application.Trades.Summary;

public sealed class GetTradeSummaryQueryHandler
    : IRequestHandler<
        GetTradeSummaryQuery,
        TradeSummaryResult>
{
    private readonly ITradeSummaryRepository _repository;

    public GetTradeSummaryQueryHandler(
        ITradeSummaryRepository repository)
    {
        _repository = repository;
    }

    public Task<TradeSummaryResult> Handle(
        GetTradeSummaryQuery request,
        CancellationToken cancellationToken)
    {
        return _repository.GetUserTradeSummaryAsync(
            request.UserId,
            request.Currency,
            request.EnergyType,
            cancellationToken);
    }
}