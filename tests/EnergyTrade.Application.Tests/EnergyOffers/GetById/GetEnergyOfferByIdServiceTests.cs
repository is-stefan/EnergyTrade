using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.EnergyOffers.GetById;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Application.Tests.EnergyOffers.GetById;

public class GetEnergyOfferByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOfferExists_ReturnsOffer()
    {
        // Arrange
        var offer = CreateValidOffer();
        var repository = new FakeEnergyOfferRepository(offer);
        var service = new GetEnergyOfferByIdService(repository);

        // Act
        var result = await service.ExecuteAsync(offer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(offer.Id, result.Id);
        Assert.Equal(offer.SellerId, result.SellerId);
        Assert.Equal(offer.EnergyType, result.EnergyType);
        Assert.Equal(offer.QuantityMWh, result.QuantityMWh);
        Assert.Equal(offer.PricePerMWh, result.PricePerMWh);
        Assert.Equal(offer.Currency, result.Currency);
        Assert.Equal(offer.Status, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfferDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeEnergyOfferRepository(null);
        var service = new GetEnergyOfferByIdService(repository);

        // Act
        var result = await service.ExecuteAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    private static EnergyOffer CreateValidOffer()
    {
        var deliveryStart = DateTimeOffset.UtcNow.AddDays(1);
        var deliveryEnd = deliveryStart.AddDays(30);

        return new EnergyOffer(
            Guid.NewGuid(),
            EnergyType.Solar,
            100m,
            80m,
            Currency.EUR,
            deliveryStart,
            deliveryEnd);
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
            if (_offer?.Id == id)
            {
                return Task.FromResult<EnergyOffer?>(_offer);
            }

            return Task.FromResult<EnergyOffer?>(null);
        }
    }
}