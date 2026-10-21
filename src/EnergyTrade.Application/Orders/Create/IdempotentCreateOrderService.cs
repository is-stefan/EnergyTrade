using MediatR;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Idempotency;

namespace EnergyTrade.Application.Orders.Create;

public sealed class IdempotentCreateOrderService
{
    private const string Operation = "CreateOrder";

    private readonly ISender _sender;
    private readonly IdempotencyService _idempotencyService;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ITransactionManager _transactionManager;

    public IdempotentCreateOrderService(
        ISender sender,
        IdempotencyService idempotencyService,
        IIdempotencyRepository idempotencyRepository,
        ITransactionManager transactionManager)
    {
        _sender = sender;
        _idempotencyService = idempotencyService;
        _idempotencyRepository = idempotencyRepository;
        _transactionManager = transactionManager;
    }

    public async Task<CreateOrderResult> ExecuteAsync(
        Guid userId,
        string idempotencyKey,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopedKey = $"{userId}:{idempotencyKey}";

        var existingResult =
            await _idempotencyService
                .GetResultAsync<CreateOrderRequest, CreateOrderResult>(
                    scopedKey,
                    Operation,
                    request,
                    cancellationToken);

        if (existingResult is not null)
        {
            return existingResult;
        }

        CreateOrderResult? result = null;

        try
        {
            await _transactionManager.ExecuteAsync(
                async transactionCancellationToken =>
                {
                    result = await _sender.Send(
                        CreateOrderCommand.FromRequest(userId, request),
                        transactionCancellationToken);

                    await _idempotencyService.SaveResultAsync(
                        scopedKey,
                        Operation,
                        request,
                        result,
                        201,
                        transactionCancellationToken);
                },
                cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            var winningRecord =
                await _idempotencyRepository.GetAsync(
                    scopedKey,
                    Operation,
                    cancellationToken);

            if (winningRecord is null)
            {
                throw;
            }

            var winningResult =
                await _idempotencyService
                    .GetResultAsync<CreateOrderRequest, CreateOrderResult>(
                        scopedKey,
                        Operation,
                        request,
                        cancellationToken);

            if (winningResult is null)
            {
                throw;
            }

            return winningResult;
        }

        return result
            ?? throw new InvalidOperationException(
                "Order creation did not return a result.");
    }
}