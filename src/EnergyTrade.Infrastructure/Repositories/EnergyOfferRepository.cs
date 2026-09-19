using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Infrastructure.Persistence;

namespace EnergyTrade.Infrastructure.Repositories;

public sealed class EnergyOfferRepository : IEnergyOfferRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public EnergyOfferRepository(EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        EnergyOffer energyOffer,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.EnergyOffers.AddAsync(
            energyOffer,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}