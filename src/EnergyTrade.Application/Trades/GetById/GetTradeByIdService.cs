using EnergyTrade.Application.Abstractions.Persistence;

namespace EnergyTrade.Application.Trades.GetById;

public sealed class GetTradeByIdService
{
    private readonly ITradeRepository _tradeRepository;

    public GetTradeByIdService(
        ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<GetTradeByIdResult?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var trade = await _tradeRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (trade is null)
        {
            return null;
        }

        return new GetTradeByIdResult(
            trade.Id,
            trade.EnergyOfferId,
            trade.SellerId,
            trade.BuyerId,
            trade.EnergyType,
            trade.QuantityMWh,
            trade.PricePerMWh,
            trade.Currency,
            trade.CreatedAt);
    }
}