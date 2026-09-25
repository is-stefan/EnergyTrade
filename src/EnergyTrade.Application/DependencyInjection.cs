using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.Extensions.DependencyInjection;
using EnergyTrade.Application.EnergyOffers.GetById;
using EnergyTrade.Application.EnergyOffers.GetAll;
using EnergyTrade.Application.EnergyOffers.Cancel;
using EnergyTrade.Application.EnergyOffers.Close;
using EnergyTrade.Application.EnergyOffers.Update;
using EnergyTrade.Application.Trades.Create;
using EnergyTrade.Application.Trades.GetById;
using EnergyTrade.Application.Trades.GetAll;
using EnergyTrade.Application.Orders.Create;
using EnergyTrade.Application.Orders.GetById;
using EnergyTrade.Application.Orders.GetAll;
using EnergyTrade.Application.Orders.Cancel;
using EnergyTrade.Application.Orders.Match;
using EnergyTrade.Application.Positions.ApplyTrade;
using EnergyTrade.Application.Positions.GetByPortfolio;
using EnergyTrade.Application.Portfolios.Create;
using EnergyTrade.Application.Portfolios.GetById;
using EnergyTrade.Application.Portfolios.GetByUser;
using EnergyTrade.Application.Portfolios.Rename;
using EnergyTrade.Application.Portfolios.Close;

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
        services.AddScoped<GetTradeByIdService>();
        services.AddScoped<GetTradesService>();
        services.AddScoped<CreateOrderService>();
        services.AddScoped<GetOrderByIdService>();
        services.AddScoped<GetOrdersService>();
        services.AddScoped<CancelOrderService>();
        services.AddScoped<MatchOrderService>();
        services.AddScoped<ApplyTradeToPositionsService>();
        services.AddScoped<GetPositionsByPortfolioService>();
        services.AddScoped<CreatePortfolioService>();
        services.AddScoped<GetPortfolioByIdService>();
        services.AddScoped<GetPortfoliosByUserService>();
        services.AddScoped<RenamePortfolioService>();
        services.AddScoped<ClosePortfolioService>();

        return services;
    }
}