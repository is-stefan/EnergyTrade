using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Infrastructure.Persistence;

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
}