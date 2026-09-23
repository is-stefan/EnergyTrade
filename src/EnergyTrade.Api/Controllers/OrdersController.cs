using EnergyTrade.Application.Orders.Create;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.Orders.GetById;
using EnergyTrade.Application.Orders.GetAll;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Application.Orders.Cancel;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderService _createOrderService;

    private readonly GetOrderByIdService _getOrderByIdService;

    private readonly GetOrdersService _getOrdersService;

    private readonly CancelOrderService _cancelOrderService;

    public OrdersController(
        CreateOrderService createOrderService,
        GetOrderByIdService getOrderByIdService,
        GetOrdersService getOrdersService,
        CancelOrderService cancelOrderService)
    {
        _createOrderService = createOrderService;
        _getOrderByIdService = getOrderByIdService;
        _getOrdersService = getOrdersService;
        _cancelOrderService = cancelOrderService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResult>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createOrderService.ExecuteAsync(
            request,
            cancellationToken);

        return Created(
            $"/api/orders/{result.Id}",
            result);
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

}