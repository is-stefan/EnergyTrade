using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Domain.Enums;
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

    public async Task<IReadOnlyList<Trade>> GetAllAsync(
        Guid? participantId = null,
        EnergyType? energyType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Trades
            .AsNoTracking()
            .AsQueryable();

        if (participantId.HasValue)
        {
            query = query.Where(
                trade =>
                    trade.SellerId == participantId.Value ||
                    trade.BuyerId == participantId.Value);
        }

        if (energyType.HasValue)
        {
            query = query.Where(
                trade => trade.EnergyType == energyType.Value);
        }

        return await query
            .OrderByDescending(trade => trade.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid? participantId = null,
        EnergyType? energyType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Trades
            .AsNoTracking()
            .AsQueryable();

        if (participantId.HasValue)
        {
            query = query.Where(
                trade =>
                    trade.SellerId == participantId.Value ||
                    trade.BuyerId == participantId.Value);
        }

        if (energyType.HasValue)
        {
            query = query.Where(
                trade => trade.EnergyType == energyType.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}