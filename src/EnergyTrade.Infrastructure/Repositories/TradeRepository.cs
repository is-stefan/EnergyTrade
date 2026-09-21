using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrade.Infrastructure.Repositories;

public sealed class TradeRepository : ITradeRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public TradeRepository(EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Trade trade,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Trades.AddAsync(
            trade,
            cancellationToken);
    }

    public async Task<Trade?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trades
            .AsNoTracking()
            .FirstOrDefaultAsync(
                trade => trade.Id == id,
                cancellationToken);
    }
}