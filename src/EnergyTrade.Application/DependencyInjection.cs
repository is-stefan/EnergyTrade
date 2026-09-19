using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyTrade.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateEnergyOfferService>();

        return services;
    }
}