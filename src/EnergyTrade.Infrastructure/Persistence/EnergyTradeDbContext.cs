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

}