using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Trades.GetById;

public sealed class GetTradeByIdQueryHandler
    : IRequestHandler<GetTradeByIdQuery, GetTradeByIdResult?>
{
    private readonly ITradeRepository _tradeRepository;

    public GetTradeByIdQueryHandler(
        ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<GetTradeByIdResult?> Handle(
        GetTradeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var trade = await _tradeRepository.GetByIdAsync(
            query.TradeId,
            cancellationToken);

        if (trade is null)
        {
            return null;
        }

        if (trade.SellerId != query.UserId &&
            trade.BuyerId != query.UserId)
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
