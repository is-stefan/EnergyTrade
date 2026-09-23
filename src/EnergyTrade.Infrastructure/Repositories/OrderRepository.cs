using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public OrderRepository(
        EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(
            order,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .FirstOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(
        Guid? buyerId = null,
        EnergyType? energyType = null,
        OrderStatus? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        if (buyerId.HasValue)
        {
            query = query.Where(
                order => order.BuyerId == buyerId.Value);
        }

        if (energyType.HasValue)
        {
            query = query.Where(
                order => order.EnergyType == energyType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(
                order => order.Status == status.Value);
        }

        return await query
            .OrderByDescending(order => order.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid? buyerId = null,
        EnergyType? energyType = null,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        if (buyerId.HasValue)
        {
            query = query.Where(
                order => order.BuyerId == buyerId.Value);
        }

        if (energyType.HasValue)
        {
            query = query.Where(
                order => order.EnergyType == energyType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(
                order => order.Status == status.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}