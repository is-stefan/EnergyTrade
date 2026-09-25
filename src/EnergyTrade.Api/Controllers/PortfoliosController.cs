using EnergyTrade.Application.Portfolios.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.Portfolios.GetById;
using EnergyTrade.Application.Portfolios.GetByUser;
using EnergyTrade.Application.Portfolios.Rename;
using EnergyTrade.Application.Portfolios.Close;
namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
public class PortfoliosController : ControllerBase
{
    private readonly CreatePortfolioService _createPortfolioService;

    private readonly GetPortfolioByIdService _getPortfolioByIdService;

    private readonly GetPortfoliosByUserService _getPortfoliosByUserService;

    private readonly RenamePortfolioService _renamePortfolioService;

    private readonly ClosePortfolioService _closePortfolioService;

    public PortfoliosController(
        CreatePortfolioService createPortfolioService,
        GetPortfolioByIdService getPortfolioByIdService,
        GetPortfoliosByUserService getPortfoliosByUserService,
        RenamePortfolioService renamePortfolioService,
        ClosePortfolioService closePortfolioService)
    {
        _createPortfolioService = createPortfolioService;
        _getPortfolioByIdService = getPortfolioByIdService;
        _getPortfoliosByUserService = getPortfoliosByUserService;
        _renamePortfolioService = renamePortfolioService;
        _closePortfolioService = closePortfolioService;
    }

    [HttpPost]
    public async Task<ActionResult<CreatePortfolioResult>> Create(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPortfolioService.ExecuteAsync(
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
        var result = await _getPortfolioByIdService.ExecuteAsync(
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<GetPortfoliosByUserResult>>> GetByUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
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
        var result = await _renamePortfolioService.ExecuteAsync(
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
        var result = await _closePortfolioService.ExecuteAsync(
            id,
            cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

}