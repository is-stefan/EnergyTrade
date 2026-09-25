using EnergyTrade.Application.Positions.GetByPortfolio;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly GetPositionsByPortfolioService _getPositionsByPortfolioService;

    public PositionsController(
        GetPositionsByPortfolioService getPositionsByPortfolioService)
    {
        _getPositionsByPortfolioService = getPositionsByPortfolioService;
    }

    [HttpGet("portfolio/{portfolioId:guid}")]
    public async Task<ActionResult<IReadOnlyList<GetPositionsByPortfolioResult>>> GetByPortfolio(
        Guid portfolioId,
        CancellationToken cancellationToken)
    {
        var result =
            await _getPositionsByPortfolioService.ExecuteAsync(
                portfolioId,
                cancellationToken);

        return Ok(result);
    }
}