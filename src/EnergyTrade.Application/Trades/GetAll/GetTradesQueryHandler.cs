using EnergyTrade.Application.Abstractions.Persistence;
using MediatR;

namespace EnergyTrade.Application.Trades.GetAll;

public sealed class GetTradesQueryHandler
    : IRequestHandler<GetTradesQuery, PagedTradesResult>
{
    private readonly ITradeRepository _tradeRepository;

    public GetTradesQueryHandler(
        ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<PagedTradesResult> Handle(
        GetTradesQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(query.UserId));
        }

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.Page),
                "Page must be greater than 0.");
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.PageSize),
                "Page size must be between 1 and 100.");
        }

        var trades = await _tradeRepository.GetAllAsync(
            query.UserId,
            query.EnergyType,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalCount = await _tradeRepository.CountAsync(
            query.UserId,
            query.EnergyType,
            cancellationToken);

        var items = trades
            .Select(trade => new GetTradesResult(
                trade.Id,
                trade.EnergyOfferId,
                trade.SellerId,
                trade.BuyerId,
                trade.EnergyType,
                trade.QuantityMWh,
                trade.PricePerMWh,
                trade.Currency,
                trade.CreatedAt))
            .ToList();

        return new PagedTradesResult(
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
