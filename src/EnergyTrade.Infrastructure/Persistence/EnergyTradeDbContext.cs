using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrade.Infrastructure.Persistence;

public class EnergyTradeDbContext : DbContext
{
    public EnergyTradeDbContext(
        DbContextOptions<EnergyTradeDbContext> options)
        : base(options)
    {
    }

    public DbSet<EnergyOffer> EnergyOffers => Set<EnergyOffer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EnergyTradeDbContext).Assembly);
    }

    public DbSet<Trade> Trades => Set<Trade>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

}