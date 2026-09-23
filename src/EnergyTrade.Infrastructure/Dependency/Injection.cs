using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using EnergyTrade.Infrastructure.Persistence;
using EnergyTrade.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyTrade.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OracleDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'OracleDatabase' was not found.");

        services.AddDbContext<EnergyTradeDbContext>(options =>
            options.UseOracle(connectionString));

        services.AddScoped<IEnergyOfferRepository, EnergyOfferRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}