using EnergyTrade.Application.Trades.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.Trades.GetById;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly CreateTradeService _createTradeService;

    private readonly GetTradeByIdService _getTradeByIdService;

    public TradesController(
        CreateTradeService createTradeService,
        GetTradeByIdService getTradeByIdService)
    {
        _createTradeService = createTradeService;
        _getTradeByIdService = getTradeByIdService;
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

}