using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Trades.GetAll;

public sealed class GetTradesService
{
    private readonly ITradeRepository _tradeRepository;

    public GetTradesService(
        ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<PagedTradesResult> ExecuteAsync(
        Guid? sellerId = null,
        Guid? buyerId = null,
        EnergyType? energyType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        var trades = await _tradeRepository.GetAllAsync(
            sellerId,
            buyerId,
            energyType,
            page,
            pageSize,
            cancellationToken);

        var totalCount = await _tradeRepository.CountAsync(
            sellerId,
            buyerId,
            energyType,
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
            page,
            pageSize,
            totalCount);
    }
}