using EnergyTrade.Api.Grpc;
using EnergyTrade.Application.Abstractions.Authentication;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Trades.GetById;
using EnergyTrade.Domain.Enums;
using Grpc.Core.Testing;
using Grpc.Core;
using MediatR;
using Moq;

namespace EnergyTrade.IntegrationTests.Grpc;

public class TradeGrpcServiceTests
{
    [Fact]
    public async Task GetTrade_WithInvalidGuid_ThrowsInvalidArgument()
    {
        var sender = new Mock<ISender>();
        var currentUserService =
            new Mock<ICurrentUserService>();

        var service = new TradeGrpcService(
            sender.Object,
            currentUserService.Object);

        var request = new GetTradeRequest
        {
            TradeId = "abc"
        };

        var context = CreateContext();

        var exception =
            await Assert.ThrowsAsync<RpcException>(
                () => service.GetTrade(
                    request,
                    context));

        Assert.Equal(
            StatusCode.InvalidArgument,
            exception.StatusCode);

        Assert.Equal(
            "Trade id must be a valid GUID.",
            exception.Status.Detail);
    }

    [Fact]
    public async Task GetTrade_WhenTradeDoesNotExist_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        var tradeId = Guid.NewGuid();

        var sender = new Mock<ISender>();

        sender
            .Setup(x => x.Send(
                It.Is<GetTradeByIdQuery>(
                    query =>
                        query.UserId == userId &&
                        query.TradeId == tradeId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new NotFoundException(
                    "Trade was not found."));

        var currentUserService =
            new Mock<ICurrentUserService>();

        currentUserService
            .SetupGet(x => x.UserId)
            .Returns(userId);

        var service = new TradeGrpcService(
            sender.Object,
            currentUserService.Object);

        var request = new GetTradeRequest
        {
            TradeId = tradeId.ToString()
        };

        var context = CreateContext();

        var exception =
            await Assert.ThrowsAsync<RpcException>(
                () => service.GetTrade(
                    request,
                    context));

        Assert.Equal(
            StatusCode.NotFound,
            exception.StatusCode);

        Assert.Equal(
            "Trade was not found.",
            exception.Status.Detail);
    }

    [Fact]
    public async Task GetTrade_WhenTradeBelongsToAnotherUser_ThrowsPermissionDenied()
    {
        var userId = Guid.NewGuid();
        var tradeId = Guid.NewGuid();

        var sender = new Mock<ISender>();

        sender
            .Setup(x => x.Send(
                It.Is<GetTradeByIdQuery>(
                    query =>
                        query.UserId == userId &&
                        query.TradeId == tradeId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ForbiddenException(
                    "The trade does not belong to the authenticated user."));

        var currentUserService =
            new Mock<ICurrentUserService>();

        currentUserService
            .SetupGet(x => x.UserId)
            .Returns(userId);

        var service = new TradeGrpcService(
            sender.Object,
            currentUserService.Object);

        var request = new GetTradeRequest
        {
            TradeId = tradeId.ToString()
        };

        var context = CreateContext();

        var exception =
            await Assert.ThrowsAsync<RpcException>(
                () => service.GetTrade(
                    request,
                    context));

        Assert.Equal(
            StatusCode.PermissionDenied,
            exception.StatusCode);

        Assert.Equal(
            "The trade does not belong to the authenticated user.",
            exception.Status.Detail);
    }

    [Fact]
    public async Task GetTrade_WithValidTrade_ReturnsMappedReply()
    {
        var userId = Guid.NewGuid();
        var tradeId = Guid.NewGuid();
        var energyOfferId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var buyerId = userId;
        var createdAt = DateTimeOffset.UtcNow;

        var result = new GetTradeByIdResult(
            tradeId,
            energyOfferId,
            sellerId,
            buyerId,
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            createdAt);

        var sender = new Mock<ISender>();

        sender
            .Setup(x => x.Send(
                It.Is<GetTradeByIdQuery>(
                    query =>
                        query.UserId == userId &&
                        query.TradeId == tradeId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var currentUserService =
            new Mock<ICurrentUserService>();

        currentUserService
            .SetupGet(x => x.UserId)
            .Returns(userId);

        var service = new TradeGrpcService(
            sender.Object,
            currentUserService.Object);

        var request = new GetTradeRequest
        {
            TradeId = tradeId.ToString()
        };

        var context = CreateContext();

        var reply = await service.GetTrade(
            request,
            context);

        Assert.Equal(
            tradeId.ToString(),
            reply.Id);

        Assert.Equal(
            energyOfferId.ToString(),
            reply.EnergyOfferId);

        Assert.Equal(
            sellerId.ToString(),
            reply.SellerId);

        Assert.Equal(
            buyerId.ToString(),
            reply.BuyerId);

        Assert.Equal(
            (int)EnergyType.Solar,
            reply.EnergyType);

        Assert.Equal(
            100d,
            reply.QuantityMwh);

        Assert.Equal(
            80d,
            reply.PricePerMwh);

        Assert.Equal(
            (int)Currency.EUR,
            reply.Currency);

        Assert.Equal(
            createdAt.ToString("O"),
            reply.CreatedAt);
    }

    private static ServerCallContext CreateContext()
    {
        return TestServerCallContext.Create(
            method: "trades.TradeService/GetTrade",
            host: "localhost",
            deadline: DateTime.UtcNow.AddMinutes(1),
            requestHeaders: new Metadata(),
            cancellationToken: CancellationToken.None,
            peer: "ipv4:127.0.0.1:12345",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => null,
            writeOptionsSetter: _ => { });
    }
}