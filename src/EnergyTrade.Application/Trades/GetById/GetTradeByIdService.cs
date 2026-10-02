using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;

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
        Guid userId,
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

        if (trade.SellerId != userId &&
            trade.BuyerId != userId)
        {
            throw new ForbiddenException(
                "The trade does not belong to the authenticated user.");
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