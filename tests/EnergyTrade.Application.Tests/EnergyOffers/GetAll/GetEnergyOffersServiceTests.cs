using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.EnergyOffers.GetAll;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.GetAll;

public class GetEnergyOffersServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOffersExist_ReturnsOffers()
    {
        // Arrange
        var firstOffer = CreateValidOffer(EnergyType.Solar, 100m);
        var secondOffer = CreateValidOffer(EnergyType.Wind, 200m);

        var repository = new FakeEnergyOfferRepository(
            new[] { firstOffer, secondOffer });

        var service = new GetEnergyOffersService(repository);

        // Act
        var result = await service.ExecuteAsync();

        // Assert
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(firstOffer.Id, result.Items[0].Id);
        Assert.Equal(firstOffer.SellerId, result.Items[0].SellerId);
        Assert.Equal(firstOffer.Status, result.Items[0].Status);

        Assert.Equal(secondOffer.Id, result.Items[1].Id);
        Assert.Equal(secondOffer.SellerId, result.Items[1].SellerId);
        Assert.Equal(secondOffer.Status, result.Items[1].Status);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoOffersExist_ReturnsEmptyList()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(
            Array.Empty<EnergyOffer>());

        var service = new GetEnergyOffersService(repository);

        // Act
        var result = await service.ExecuteAsync();

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
        var offers = new List<EnergyOffer>
        {
            CreateOffer(100m),
            CreateOffer(200m),
            CreateOffer(300m),
            CreateOffer(400m),
            CreateOffer(500m)
        };

        var repository = new FakeEnergyOfferRepository(offers);
        var service = new GetEnergyOffersService(repository);

        // Act
        var result = await service.ExecuteAsync(
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

    private static EnergyOffer CreateValidOffer(
        EnergyType energyType,
        decimal quantityMWh)
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            energyType,
            quantityMWh,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
    }

    private static EnergyOffer CreateOffer(decimal quantityMWh)
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnergyType.Solar,
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
            OfferStatus? status = null,
            EnergyType? energyType = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _offers.AsEnumerable();

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
            OfferStatus? status = null,
            EnergyType? energyType = null,
            CancellationToken cancellationToken = default)
        {
            var query = _offers.AsEnumerable();

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

    [Fact]
    public async Task ExecuteAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(
            Array.Empty<EnergyOffer>());

        var service = new GetEnergyOffersService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(page: 0));
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
            () => service.ExecuteAsync(pageSize: 101));
    }

}