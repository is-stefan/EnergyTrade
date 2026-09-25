using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Orders.Match;
using EnergyTrade.Application.Positions.ApplyTrade;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.Orders.Match;

public class MatchOrderServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMatchingOfferExists_CreatesTradeAndUpdatesStates()
    {
        // Arrange
        var order = CreateOrder();
        var offer = CreateOffer();

        var orderRepository = new FakeOrderRepository(order);
        var energyOfferRepository = new FakeEnergyOfferRepository(offer);
        var tradeRepository = new FakeTradeRepository();
        var positionRepository = new FakePositionRepository();

        var applyTradeToPositionsService =
            new ApplyTradeToPositionsService(positionRepository);

        var service = new MatchOrderService(
            orderRepository,
            energyOfferRepository,
            tradeRepository,
            applyTradeToPositionsService);

        // Act
        var result = await service.ExecuteAsync(order.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Matched);
        Assert.Equal(offer.Id, result.EnergyOfferId);
        Assert.NotNull(result.TradeId);

        Assert.Equal(OrderStatus.Filled, order.Status);
        Assert.Equal(OfferStatus.Closed, offer.Status);

        Assert.NotNull(tradeRepository.AddedTrade);
        Assert.Equal(1, tradeRepository.AddCallCount);

        Assert.Equal(2, positionRepository.Positions.Count);

        var buyerPosition = positionRepository.Positions
            .Single(x => x.PortfolioId == order.PortfolioId);

        var sellerPosition = positionRepository.Positions
            .Single(x => x.PortfolioId == offer.PortfolioId);

        Assert.Equal(100m, buyerPosition.QuantityMWh);
        Assert.Equal(-100m, sellerPosition.QuantityMWh);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderRepository = new FakeOrderRepository(null);
        var energyOfferRepository = new FakeEnergyOfferRepository(null);
        var tradeRepository = new FakeTradeRepository();
        var positionRepository = new FakePositionRepository();

        var applyTradeToPositionsService =
            new ApplyTradeToPositionsService(positionRepository);

        var service = new MatchOrderService(
            orderRepository,
            energyOfferRepository,
            tradeRepository,
            applyTradeToPositionsService);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
        Assert.Equal(0, tradeRepository.AddCallCount);
        Assert.Empty(positionRepository.Positions);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoMatchingOfferExists_ReturnsNotMatched()
    {
        // Arrange
        var order = CreateOrder();

        var orderRepository = new FakeOrderRepository(order);
        var energyOfferRepository = new FakeEnergyOfferRepository(null);
        var tradeRepository = new FakeTradeRepository();
        var positionRepository = new FakePositionRepository();

        var applyTradeToPositionsService =
            new ApplyTradeToPositionsService(positionRepository);

        var service = new MatchOrderService(
            orderRepository,
            energyOfferRepository,
            tradeRepository,
            applyTradeToPositionsService);

        // Act
        var result = await service.ExecuteAsync(order.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Matched);
        Assert.Null(result.TradeId);
        Assert.Null(result.EnergyOfferId);

        Assert.Equal(OrderStatus.Open, order.Status);
        Assert.Equal(0, tradeRepository.AddCallCount);
        Assert.Empty(positionRepository.Positions);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderIsNotOpen_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateOrder();
        order.Cancel();

        var offer = CreateOffer();

        var orderRepository = new FakeOrderRepository(order);
        var energyOfferRepository = new FakeEnergyOfferRepository(offer);
        var tradeRepository = new FakeTradeRepository();
        var positionRepository = new FakePositionRepository();

        var applyTradeToPositionsService =
            new ApplyTradeToPositionsService(positionRepository);

        var service = new MatchOrderService(
            orderRepository,
            energyOfferRepository,
            tradeRepository,
            applyTradeToPositionsService);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(order.Id));

        Assert.Equal(0, tradeRepository.AddCallCount);
        Assert.Empty(positionRepository.Positions);
    }

    private static Order CreateOrder()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(10);
        var deliveryEnd = deliveryStart.AddDays(10);

        return new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            90m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }

    private static EnergyOffer CreateOffer()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(5);
        var deliveryEnd = deliveryStart.AddDays(20);

        return new EnergyOffer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
            150m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }

    private sealed class FakeOrderRepository
        : IOrderRepository
    {
        private readonly Order? _order;

        public FakeOrderRepository(Order? order)
        {
            _order = order;
        }

        public Task AddAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_order?.Id == id)
            {
                return Task.FromResult<Order?>(_order);
            }

            return Task.FromResult<Order?>(null);
        }

        public Task<IReadOnlyList<Order>> GetAllAsync(
            Guid? buyerId = null,
            EnergyType? energyType = null,
            OrderStatus? status = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Order>>(
                Array.Empty<Order>());
        }

        public Task<int> CountAsync(
            Guid? buyerId = null,
            EnergyType? energyType = null,
            OrderStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEnergyOfferRepository
        : IEnergyOfferRepository
    {
        private readonly EnergyOffer? _offer;

        public FakeEnergyOfferRepository(EnergyOffer? offer)
        {
            _offer = offer;
        }

        public Task AddAsync(
            EnergyOffer energyOffer,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<EnergyOffer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EnergyOffer?>(null);
        }

        public Task<IReadOnlyList<EnergyOffer>> GetAllAsync(
            OfferStatus? status = null,
            EnergyType? energyType = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EnergyOffer>>(
                Array.Empty<EnergyOffer>());
        }

        public Task<int> CountAsync(
            OfferStatus? status = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<EnergyOffer?> FindMatchingAsync(
            Guid buyerId,
            EnergyType energyType,
            decimal quantityMWh,
            decimal maxPricePerMWh,
            Currency currency,
            DateTimeOffset deliveryStart,
            DateTimeOffset deliveryEnd,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_offer);
        }
    }

    private sealed class FakeTradeRepository
        : ITradeRepository
    {
        public Trade? AddedTrade { get; private set; }

        public int AddCallCount { get; private set; }

        public Task AddAsync(
            Trade trade,
            CancellationToken cancellationToken = default)
        {
            AddedTrade = trade;
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task<Trade?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Trade?>(null);
        }

        public Task<IReadOnlyList<Trade>> GetAllAsync(
            Guid? sellerId = null,
            Guid? buyerId = null,
            EnergyType? energyType = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Trade>>(
                Array.Empty<Trade>());
        }

        public Task<int> CountAsync(
            Guid? sellerId = null,
            Guid? buyerId = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }

    private sealed class FakePositionRepository
        : IPositionRepository
    {
        public List<Position> Positions { get; } = [];

        public Task<Position?> GetByPortfolioAndEnergyTypeAsync(
            Guid portfolioId,
            EnergyType energyType,
            CancellationToken cancellationToken = default)
        {
            var position = Positions.FirstOrDefault(x =>
                x.PortfolioId == portfolioId &&
                x.EnergyType == energyType);

            return Task.FromResult(position);
        }

        public Task<IReadOnlyList<Position>> GetByPortfolioIdAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Position> positions = Positions
                .Where(x => x.PortfolioId == portfolioId)
                .ToList();

            return Task.FromResult(positions);
        }

        public Task AddAsync(
            Position position,
            CancellationToken cancellationToken = default)
        {
            Positions.Add(position);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

    }
}