using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrade.Infrastructure.Persistence.Repositories;

public sealed class PositionRepository : IPositionRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public PositionRepository(EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Position position,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Positions.AddAsync(
            position,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Position?> GetByPortfolioAndEnergyTypeAsync(
        Guid portfolioId,
        EnergyType energyType,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions
            .FirstOrDefaultAsync(
                position =>
                    position.PortfolioId == portfolioId &&
                    position.EnergyType == energyType,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Position>> GetByPortfolioIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions
            .AsNoTracking()
            .Where(position => position.PortfolioId == portfolioId)
            .OrderBy(position => position.EnergyType)
            .ToListAsync(cancellationToken);
    }
    
}