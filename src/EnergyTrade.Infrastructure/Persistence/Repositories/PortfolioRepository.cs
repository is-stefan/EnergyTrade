using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrade.Infrastructure.Persistence.Repositories;

public sealed class PortfolioRepository
    : IPortfolioRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public PortfolioRepository(
        EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Portfolio portfolio,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Portfolios.AddAsync(
            portfolio,
            cancellationToken);
    }

    public async Task<Portfolio?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .FirstOrDefaultAsync(
                portfolio => portfolio.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .AsNoTracking()
            .Where(portfolio => portfolio.UserId == userId)
            .OrderBy(portfolio => portfolio.Name)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}