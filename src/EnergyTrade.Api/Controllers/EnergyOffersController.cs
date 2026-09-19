using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/energy-offers")]
public class EnergyOffersController : ControllerBase
{
    private readonly CreateEnergyOfferService _createEnergyOfferService;

    public EnergyOffersController(
        CreateEnergyOfferService createEnergyOfferService)
    {
        _createEnergyOfferService = createEnergyOfferService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateEnergyOfferResult>> Create(
        CreateEnergyOfferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createEnergyOfferService.ExecuteAsync(
            request,
            cancellationToken);

        return Created(
            $"/api/energy-offers/{result.Id}",
            result);
    }
}