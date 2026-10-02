using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrade.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public UserRepository(
        EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(
            user,
            cancellationToken);
    }

    public async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}