using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.EnergyOffers.GetById;
using EnergyTrade.Application.EnergyOffers.GetAll;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/energy-offers")]
public class EnergyOffersController : ControllerBase
{
    private readonly CreateEnergyOfferService _createEnergyOfferService;

    private readonly GetEnergyOfferByIdService _getEnergyOfferByIdService;

    private readonly GetEnergyOffersService _getEnergyOffersService;

    public EnergyOffersController(
        CreateEnergyOfferService createEnergyOfferService,
        GetEnergyOfferByIdService getEnergyOfferByIdService,
        GetEnergyOffersService getEnergyOffersService)
    {
        _createEnergyOfferService = createEnergyOfferService;
        _getEnergyOfferByIdService = getEnergyOfferByIdService;
        _getEnergyOffersService = getEnergyOffersService;
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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetEnergyOffersResult>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _getEnergyOffersService.ExecuteAsync(
            cancellationToken);

        return Ok(result);
    }

}