using EnergyTrade.Application.Trades.Create;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly CreateTradeService _createTradeService;

    public TradesController(
        CreateTradeService createTradeService)
    {
        _createTradeService = createTradeService;
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
}