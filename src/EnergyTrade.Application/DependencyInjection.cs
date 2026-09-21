using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.Extensions.DependencyInjection;
using EnergyTrade.Application.EnergyOffers.GetById;
using EnergyTrade.Application.EnergyOffers.GetAll;
using EnergyTrade.Application.EnergyOffers.Cancel;
using EnergyTrade.Application.EnergyOffers.Close;
using EnergyTrade.Application.EnergyOffers.Update;
using EnergyTrade.Application.Trades.Create;

namespace EnergyTrade.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateEnergyOfferService>();
        services.AddScoped<GetEnergyOfferByIdService>();
        services.AddScoped<GetEnergyOffersService>();
        services.AddScoped<CancelEnergyOfferService>();
        services.AddScoped<CloseEnergyOfferService>();
        services.AddScoped<UpdateEnergyOfferService>();
        services.AddScoped<CreateTradeService>();
        
        return services;
    }
}