using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<EnergyOffer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EnergyOffers
            .FirstOrDefaultAsync(
                offer => offer.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<EnergyOffer>> GetAllAsync(
        OfferStatus? status = null,
        EnergyType? energyType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EnergyOffers
            .AsNoTracking()
            .AsQueryable();

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

        return await query
            .OrderByDescending(offer => offer.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        OfferStatus? status = null,
        EnergyType? energyType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EnergyOffers
            .AsNoTracking()
            .AsQueryable();

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

        return await query.CountAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

}