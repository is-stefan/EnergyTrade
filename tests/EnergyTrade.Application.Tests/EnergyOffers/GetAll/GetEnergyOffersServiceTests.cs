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
        Assert.Equal(2, result.Count);

        Assert.Equal(firstOffer.Id, result[0].Id);
        Assert.Equal(EnergyType.Solar, result[0].EnergyType);
        Assert.Equal(100m, result[0].QuantityMWh);

        Assert.Equal(secondOffer.Id, result[1].Id);
        Assert.Equal(EnergyType.Wind, result[1].EnergyType);
        Assert.Equal(200m, result[1].QuantityMWh);
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
        Assert.Empty(result);
    }

    private static EnergyOffer CreateValidOffer(
        EnergyType energyType,
        decimal quantityMWh)
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
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
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_offers);
        }
    }
}