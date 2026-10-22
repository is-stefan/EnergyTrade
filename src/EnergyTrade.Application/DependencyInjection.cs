using EnergyTrade.Application.Auth.Login;
using EnergyTrade.Application.Auth.Register;
using EnergyTrade.Application.EnergyOffers.Create;
using EnergyTrade.Application.Idempotency;
using EnergyTrade.Application.Orders.Create;
using EnergyTrade.Application.Positions.ApplyTrade;
using EnergyTrade.Application.Positions.GetByPortfolio;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyTrade.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<CreateOrderCommandHandler>());

        services.AddScoped<ApplyTradeToPositionsService>();

        services.AddScoped<IdempotencyService>();
        services.AddScoped<IdempotentCreateOrderService>();
        services.AddScoped<IdempotentCreateEnergyOfferService>();

        return services;
    }
}
