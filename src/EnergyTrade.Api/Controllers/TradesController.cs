using EnergyTrade.Application.Trades.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.Trades.GetById;
using EnergyTrade.Application.Trades.GetAll;
using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly CreateTradeService _createTradeService;

    private readonly GetTradeByIdService _getTradeByIdService;

    private readonly GetTradesService _getTradesService;

    public TradesController(
        CreateTradeService createTradeService,
        GetTradeByIdService getTradeByIdService,
        GetTradesService getTradesService)
    {
        _createTradeService = createTradeService;
        _getTradeByIdService = getTradeByIdService;
        _getTradesService = getTradesService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateTradeResult>> Create(
        CreateTradeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createTradeService.ExecuteAsync(
            request,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Created(
            $"/api/trades/{result.Id}",
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetTradeByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getTradeByIdService.ExecuteAsync(
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedTradesResult>> GetAll(
        [FromQuery] Guid? sellerId,
        [FromQuery] Guid? buyerId,
        [FromQuery] EnergyType? energyType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _getTradesService.ExecuteAsync(
            sellerId,
            buyerId,
            energyType,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

}