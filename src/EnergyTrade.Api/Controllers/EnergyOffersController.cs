using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.EnergyOffers.GetById;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/energy-offers")]
public class EnergyOffersController : ControllerBase
{
    private readonly CreateEnergyOfferService _createEnergyOfferService;

    private readonly GetEnergyOfferByIdService _getEnergyOfferByIdService;

    public EnergyOffersController(
        CreateEnergyOfferService createEnergyOfferService,
        GetEnergyOfferByIdService getEnergyOfferByIdService)
    {
        _createEnergyOfferService = createEnergyOfferService;
        _getEnergyOfferByIdService = getEnergyOfferByIdService;
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetEnergyOfferByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getEnergyOfferByIdService.ExecuteAsync(
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

}