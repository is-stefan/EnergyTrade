using EnergyTrade.Application.Orders.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.Orders.GetById;
using EnergyTrade.Application.Orders.GetAll;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Application.Orders.Cancel;
using EnergyTrade.Application.Orders.Match;
using EnergyTrade.Application.Idempotency;
using EnergyTrade.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace EnergyTrade.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderService _createOrderService;

    private readonly GetOrderByIdService _getOrderByIdService;

    private readonly GetOrdersService _getOrdersService;

    private readonly CancelOrderService _cancelOrderService;

    private readonly MatchOrderService _matchOrderService;

    private readonly IdempotentCreateOrderService _idempotentCreateOrderService;

    private readonly ICurrentUserService _currentUserService;

    public OrdersController(
        CreateOrderService createOrderService,
        GetOrderByIdService getOrderByIdService,
        GetOrdersService getOrdersService,
        CancelOrderService cancelOrderService,
        MatchOrderService matchOrderService,
        IdempotentCreateOrderService idempotentCreateOrderService,
        ICurrentUserService currentUserService)
    {
        _createOrderService = createOrderService;
        _getOrderByIdService = getOrderByIdService;
        _getOrdersService = getOrdersService;
        _cancelOrderService = cancelOrderService;
        _matchOrderService = matchOrderService;
        _idempotentCreateOrderService = idempotentCreateOrderService;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetOrderByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _getOrderByIdService.ExecuteAsync(
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
    public async Task<ActionResult<PagedOrdersResult>> GetAll(
        [FromQuery] EnergyType? energyType,
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var result = await _getOrdersService.ExecuteAsync(
            userId,
            energyType,
            status,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
public async Task<IActionResult> Cancel(
    Guid id,
    CancellationToken cancellationToken)
{
    var userId = _currentUserService.UserId;

    var success = await _cancelOrderService.ExecuteAsync(
        userId,
        id,
        cancellationToken);

    if (!success)
    {
        return NotFound();
    }

    return NoContent();
}

    [HttpPost("{id:guid}/match")]
    public async Task<ActionResult<MatchOrderResult>> Match(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var result = await _matchOrderService.ExecuteAsync(
            userId,
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        CreateOrderResult result;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            result = await _idempotentCreateOrderService.ExecuteAsync(
                userId,
                idempotencyKey,
                request,
                cancellationToken);
        }
        else
        {
            result = await _createOrderService.ExecuteAsync(
                userId,
                request,
                cancellationToken);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

}