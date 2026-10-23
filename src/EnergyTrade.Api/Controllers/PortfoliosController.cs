using EnergyTrade.Application.Abstractions.Authentication;
using EnergyTrade.Application.Portfolios.Close;
using EnergyTrade.Application.Portfolios.Create;
using EnergyTrade.Application.Portfolios.GetById;
using EnergyTrade.Application.Portfolios.GetByUser;
using EnergyTrade.Application.Portfolios.Rename;
using EnergyTrade.Application.Portfolios.Exposure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
[Authorize]
public class PortfoliosController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public PortfoliosController(
        ISender sender,
        ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<CreatePortfolioResult>> Create(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            CreatePortfolioCommand.FromRequest(
                userId,
                request),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPortfolioByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            new GetPortfolioByIdQuery(
                userId,
                id),
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<GetPortfoliosByUserResult>>> GetMine(
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            new GetPortfoliosByUserQuery(userId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/rename")]
    public async Task<IActionResult> Rename(
        Guid id,
        RenamePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            RenamePortfolioCommand.FromRequest(
                userId,
                id,
                request),
            cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            new ClosePortfolioCommand(
                userId,
                id),
            cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id:guid}/exposure")]
    public async Task<ActionResult<decimal>> GetExposure(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _sender.Send(
            new GetPortfolioExposureQuery(
                userId,
                id),
            cancellationToken);

        return Ok(result);
    }
}
