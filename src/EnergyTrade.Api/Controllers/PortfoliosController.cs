using EnergyTrade.Application.Abstractions.Authentication;
using EnergyTrade.Application.Portfolios.Close;
using EnergyTrade.Application.Portfolios.Create;
using EnergyTrade.Application.Portfolios.GetById;
using EnergyTrade.Application.Portfolios.GetByUser;
using EnergyTrade.Application.Portfolios.Rename;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
[Authorize]
public class PortfoliosController : ControllerBase
{
    private readonly CreatePortfolioService _createPortfolioService;
    private readonly GetPortfolioByIdService _getPortfolioByIdService;
    private readonly GetPortfoliosByUserService _getPortfoliosByUserService;
    private readonly RenamePortfolioService _renamePortfolioService;
    private readonly ClosePortfolioService _closePortfolioService;
    private readonly ICurrentUserService _currentUserService;

    public PortfoliosController(
        CreatePortfolioService createPortfolioService,
        GetPortfolioByIdService getPortfolioByIdService,
        GetPortfoliosByUserService getPortfoliosByUserService,
        RenamePortfolioService renamePortfolioService,
        ClosePortfolioService closePortfolioService,
        ICurrentUserService currentUserService)
    {
        _createPortfolioService = createPortfolioService;
        _getPortfolioByIdService = getPortfolioByIdService;
        _getPortfoliosByUserService = getPortfoliosByUserService;
        _renamePortfolioService = renamePortfolioService;
        _closePortfolioService = closePortfolioService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<CreatePortfolioResult>> Create(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _createPortfolioService.ExecuteAsync(
            userId,
            request,
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

        var result = await _getPortfolioByIdService.ExecuteAsync(
            userId,
            id,
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

        var result = await _getPortfoliosByUserService.ExecuteAsync(
            userId,
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

        var result = await _renamePortfolioService.ExecuteAsync(
            userId,
            id,
            request,
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

        var result = await _closePortfolioService.ExecuteAsync(
            userId,
            id,
            cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}