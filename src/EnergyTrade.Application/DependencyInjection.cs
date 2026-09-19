using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.Extensions.DependencyInjection;
using EnergyTrade.Application.EnergyOffers.GetById;

namespace EnergyTrade.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateEnergyOfferService>();
        services.AddScoped<GetEnergyOfferByIdService>();

        return services;
    }
}