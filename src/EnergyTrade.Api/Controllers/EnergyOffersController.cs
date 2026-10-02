using EnergyTrade.Application.EnergyOffers.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.EnergyOffers.GetById;
using EnergyTrade.Application.EnergyOffers.GetAll;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Application.EnergyOffers.Cancel;
using EnergyTrade.Application.EnergyOffers.Close;
using EnergyTrade.Application.EnergyOffers.Update;
using EnergyTrade.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace EnergyTrade.Api.Controllers;

[Authorize]
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

    private readonly IdempotentCreateEnergyOfferService _idempotentCreateEnergyOfferService;

    private readonly ICurrentUserService _currentUserService;

    public EnergyOffersController(
        CreateEnergyOfferService createEnergyOfferService,
        GetEnergyOfferByIdService getEnergyOfferByIdService,
        GetEnergyOffersService getEnergyOffersService,
        CloseEnergyOfferService closeEnergyOfferService,
        CancelEnergyOfferService cancelEnergyOfferService,
        UpdateEnergyOfferService updateEnergyOfferService,
        IdempotentCreateEnergyOfferService idempotentCreateEnergyOfferService,
        ICurrentUserService currentUserService)
    {
        _createEnergyOfferService = createEnergyOfferService;
        _getEnergyOfferByIdService = getEnergyOfferByIdService;
        _getEnergyOffersService = getEnergyOffersService;
        _closeEnergyOfferService = closeEnergyOfferService;
        _cancelEnergyOfferService = cancelEnergyOfferService;
        _updateEnergyOfferService = updateEnergyOfferService;
        _idempotentCreateEnergyOfferService = idempotentCreateEnergyOfferService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateEnergyOfferRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        CreateEnergyOfferResult result;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            result =
                await _idempotentCreateEnergyOfferService.ExecuteAsync(
                    userId,
                    idempotencyKey,
                    request,
                    cancellationToken);
        }
        else
        {
            result =
                await _createEnergyOfferService.ExecuteAsync(
                    userId,
                    request,
                    cancellationToken);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetEnergyOfferByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _getEnergyOfferByIdService.ExecuteAsync(
            userId,
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedEnergyOffersResult>> GetAll(
        [FromQuery] OfferStatus? status,
        [FromQuery] EnergyType? energyType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var result = await _getEnergyOffersService.ExecuteAsync(
            userId,
            status,
            energyType,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var success = await _closeEnergyOfferService.ExecuteAsync(
            userId,
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
        var userId = _currentUserService.UserId;

        var success = await _cancelEnergyOfferService.ExecuteAsync(
            userId,
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
        var userId = _currentUserService.UserId;

        var success = await _updateEnergyOfferService.ExecuteAsync(
            userId,
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