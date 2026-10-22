using EnergyTrade.Application.Abstractions.Authentication;
using EnergyTrade.Application.Positions.GetByPortfolio;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/positions")]
[Authorize]
public sealed class PositionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public PositionsController(
        ISender sender,
        ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    [HttpGet("portfolio/{portfolioId:guid}")]
    public async Task<ActionResult<IReadOnlyList<GetPositionsByPortfolioResult>>> GetByPortfolio(
        Guid portfolioId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            new GetPositionsByPortfolioQuery(
                userId,
                portfolioId),
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
