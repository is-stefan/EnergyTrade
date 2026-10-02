using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.EnergyOffers.GetAll;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.GetAll;

public class GetEnergyOffersServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOffersExist_ReturnsOnlyAuthenticatedUserOffers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var firstOffer = CreateOffer(
            userId,
            EnergyType.Solar,
            100m);

        var secondOffer = CreateOffer(
            userId,
            EnergyType.Wind,
            200m);

        var otherUserOffer = CreateOffer(
            otherUserId,
            EnergyType.Solar,
            300m);

        var repository = new FakeEnergyOfferRepository(
            new[] { firstOffer, secondOffer, otherUserOffer });

        var service = new GetEnergyOffersService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(firstOffer.Id, result.Items[0].Id);
        Assert.Equal(firstOffer.SellerId, result.Items[0].SellerId);

        Assert.Equal(secondOffer.Id, result.Items[1].Id);
        Assert.Equal(secondOffer.SellerId, result.Items[1].SellerId);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoOffersExist_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var repository = new FakeEnergyOfferRepository(
            Array.Empty<EnergyOffer>());

        var service = new GetEnergyOffersService(repository);

        // Act
        var result = await service.ExecuteAsync(userId);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ReturnsRequestedPage()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var offers = new List<EnergyOffer>
        {
            CreateOffer(userId, EnergyType.Solar, 100m),
            CreateOffer(userId, EnergyType.Solar, 200m),
            CreateOffer(userId, EnergyType.Solar, 300m),
            CreateOffer(userId, EnergyType.Solar, 400m),
            CreateOffer(userId, EnergyType.Solar, 500m)
        };

        var repository = new FakeEnergyOfferRepository(offers);
        var service = new GetEnergyOffersService(repository);

        // Act
        var result = await service.ExecuteAsync(
            userId,
            page: 2,
            pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);

        Assert.Equal(300m, result.Items[0].QuantityMWh);
        Assert.Equal(400m, result.Items[1].QuantityMWh);
    }

    [Fact]
    public async Task ExecuteAsync_WithEnergyTypeFilter_ReturnsOnlyMatchingOffers()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var offers = new List<EnergyOffer>
        {
            CreateOffer(userId, EnergyType.Solar, 100m),
            CreateOffer(userId, EnergyType.Wind, 200m),
            CreateOffer(Guid.NewGuid(), EnergyType.Wind, 300m)
        };

        var repository = new FakeEnergyOfferRepository(offers);
        var service = new GetEnergyOffersService(repository);

        // Act
        var result = await service.ExecuteAsync(
            userId,
            energyType: EnergyType.Wind);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(EnergyType.Wind, result.Items[0].EnergyType);
        Assert.Equal(userId, result.Items[0].SellerId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(
            Array.Empty<EnergyOffer>());

        var service = new GetEnergyOffersService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(Guid.Empty));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(
            Array.Empty<EnergyOffer>());

        var service = new GetEnergyOffersService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                Guid.NewGuid(),
                page: 0));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(
            Array.Empty<EnergyOffer>());

        var service = new GetEnergyOffersService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                Guid.NewGuid(),
                pageSize: 101));
    }

    private static EnergyOffer CreateOffer(
        Guid sellerId,
        EnergyType energyType,
        decimal quantityMWh)
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            sellerId,
            Guid.NewGuid(),
            energyType,
            quantityMWh,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }

    private sealed class FakeEnergyOfferRepository
        : IEnergyOfferRepository
    {
        private readonly IReadOnlyList<EnergyOffer> _offers;

        public FakeEnergyOfferRepository(
            IReadOnlyList<EnergyOffer> offers)
        {
            _offers = offers;
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
            var offer = _offers.FirstOrDefault(
                offer => offer.Id == id);

            return Task.FromResult(offer);
        }

        public Task<IReadOnlyList<EnergyOffer>> GetAllAsync(
            Guid? sellerId = null,
            OfferStatus? status = null,
            EnergyType? energyType = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _offers.AsEnumerable();

            if (sellerId.HasValue)
            {
                query = query.Where(
                    offer => offer.SellerId == sellerId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(
                    offer => offer.Status == status.Value);
            }

            if (energyType.HasValue)
            {
                query = query.Where(
                    offer => offer.EnergyType == energyType.Value);
            }

            var result = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<IReadOnlyList<EnergyOffer>>(result);
        }

        public Task<int> CountAsync(
            Guid? sellerId = null,
            OfferStatus? status = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            var query = _offers.AsEnumerable();

            if (sellerId.HasValue)
            {
                query = query.Where(
                    offer => offer.SellerId == sellerId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(
                    offer => offer.Status == status.Value);
            }

            if (energyType.HasValue)
            {
                query = query.Where(
                    offer => offer.EnergyType == energyType.Value);
            }

            return Task.FromResult(query.Count());
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
            return Task.FromResult<EnergyOffer?>(null);
        }
    }
}