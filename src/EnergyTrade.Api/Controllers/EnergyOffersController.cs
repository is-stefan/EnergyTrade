using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.EnergyOffers.GetById;
using EnergyTrade.Application.EnergyOffers.GetAll;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Application.EnergyOffers.Cancel;
using EnergyTrade.Application.EnergyOffers.Close;
using EnergyTrade.Application.EnergyOffers.Update;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/energy-offers")]
public class EnergyOffersController : ControllerBase
{
    private readonly CreateEnergyOfferService _createEnergyOfferService;

    private readonly GetEnergyOfferByIdService _getEnergyOfferByIdService;

    private readonly GetEnergyOffersService _getEnergyOffersService;

    private readonly CancelEnergyOfferService _cancelEnergyOfferService;

    private readonly CloseEnergyOfferService _closeEnergyOfferService;

    private readonly UpdateEnergyOfferService _updateEnergyOfferService;

    public EnergyOffersController(
        CreateEnergyOfferService createEnergyOfferService,
        GetEnergyOfferByIdService getEnergyOfferByIdService,
        GetEnergyOffersService getEnergyOffersService,
        CloseEnergyOfferService closeEnergyOfferService,
        CancelEnergyOfferService cancelEnergyOfferService,
        UpdateEnergyOfferService updateEnergyOfferService)
    {
        _createEnergyOfferService = createEnergyOfferService;
        _getEnergyOfferByIdService = getEnergyOfferByIdService;
        _getEnergyOffersService = getEnergyOffersService;
        _closeEnergyOfferService = closeEnergyOfferService;
        _cancelEnergyOfferService = cancelEnergyOfferService;
        _updateEnergyOfferService = updateEnergyOfferService;
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
        [FromQuery] OfferStatus? status,
        [FromQuery] EnergyType? energyType,
        CancellationToken cancellationToken)
    {
        var result = await _getEnergyOffersService.ExecuteAsync(
            status,
            energyType,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var success = await _closeEnergyOfferService.ExecuteAsync(
            id,
            cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var success = await _cancelEnergyOfferService.ExecuteAsync(
            id,
            cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateEnergyOfferRequest request,
        CancellationToken cancellationToken)
    {
        var success = await _updateEnergyOfferService.ExecuteAsync(
            id,
            request,
            cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

}