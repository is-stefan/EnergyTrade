using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.GetAll;

public sealed record GetEnergyOffersQuery(
    Guid UserId,
    OfferStatus? Status = null,
    EnergyType? EnergyType = null,
    int Page = 1,
    int PageSize = 10)
    : IRequest<PagedEnergyOffersResult>;