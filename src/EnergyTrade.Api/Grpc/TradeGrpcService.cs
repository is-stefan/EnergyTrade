using EnergyTrade.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Trades.GetById;
using Grpc.Core;
using MediatR;

namespace EnergyTrade.Api.Grpc;

[Authorize]
public sealed class TradeGrpcService : TradeService.TradeServiceBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public TradeGrpcService(
        ISender sender,
        ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    public override async Task<TradeReply> GetTrade(
        GetTradeRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.TradeId, out var tradeId))
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Trade id must be a valid GUID."));
        }

        try
        {
            var userId = _currentUserService.UserId;

            var trade = await _sender.Send(
                new GetTradeByIdQuery(
                    userId,
                    tradeId),
                context.CancellationToken);

            if (trade is null)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.NotFound,
                        "Trade was not found."));
            }

            return new TradeReply
            {
                Id = trade.Id.ToString(),
                EnergyOfferId = trade.EnergyOfferId.ToString(),
                SellerId = trade.SellerId.ToString(),
                BuyerId = trade.BuyerId.ToString(),
                EnergyType = (int)trade.EnergyType,
                QuantityMwh = (double)trade.QuantityMWh,
                PricePerMwh = (double)trade.PricePerMWh,
                Currency = (int)trade.Currency,
                CreatedAt = trade.CreatedAt.ToString("O")
            };
        }
        catch (NotFoundException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    exception.Message));
        }
        catch (ForbiddenException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.PermissionDenied,
                    exception.Message));
        }
    }
}