using EnergyTrade.Application.Orders.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.Orders.GetById;
using EnergyTrade.Application.Orders.GetAll;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Application.Orders.Cancel;
using EnergyTrade.Application.Orders.Match;
using EnergyTrade.Application.Idempotency;

namespace EnergyTrade.Api.Controllers;

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

    public OrdersController(
        CreateOrderService createOrderService,
        GetOrderByIdService getOrderByIdService,
        GetOrdersService getOrdersService,
        CancelOrderService cancelOrderService,
        MatchOrderService matchOrderService,
        IdempotentCreateOrderService idempotentCreateOrderService)
    {
        _createOrderService = createOrderService;
        _getOrderByIdService = getOrderByIdService;
        _getOrdersService = getOrdersService;
        _cancelOrderService = cancelOrderService;
        _matchOrderService = matchOrderService;
        _idempotentCreateOrderService = idempotentCreateOrderService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetOrderByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getOrderByIdService.ExecuteAsync(
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
        [FromQuery] Guid? buyerId,
        [FromQuery] EnergyType? energyType,
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _getOrdersService.ExecuteAsync(
            buyerId,
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
        var success = await _cancelOrderService.ExecuteAsync(
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
        var result = await _matchOrderService.ExecuteAsync(
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
        CreateOrderResult result;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            result = await _idempotentCreateOrderService.ExecuteAsync(
                idempotencyKey,
                request,
                cancellationToken);
        }
        else
        {
            result = await _createOrderService.ExecuteAsync(
                request,
                cancellationToken);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

}