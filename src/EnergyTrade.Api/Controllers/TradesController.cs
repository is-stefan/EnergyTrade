using EnergyTrade.Application.Abstractions.Authentication;
using EnergyTrade.Application.Trades.GetAll;
using EnergyTrade.Application.Trades.GetById;
using EnergyTrade.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/trades")]
[Authorize]
public class TradesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public TradesController(
        ISender sender,
        ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetTradeByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            new GetTradeByIdQuery(userId, id),
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedTradesResult>> GetAll(
        [FromQuery] EnergyType? energyType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            new GetTradesQuery(
                userId,
                energyType,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }
}
