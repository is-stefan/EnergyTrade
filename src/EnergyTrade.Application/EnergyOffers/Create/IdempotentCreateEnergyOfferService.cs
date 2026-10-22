using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Idempotency;
using MediatR;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed class IdempotentCreateEnergyOfferService
{
    private const string Operation = "CreateEnergyOffer";

    private readonly ISender _sender;
    private readonly IdempotencyService _idempotencyService;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ITransactionManager _transactionManager;

    public IdempotentCreateEnergyOfferService(
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

    public async Task<CreateEnergyOfferResult> ExecuteAsync(
        Guid userId,
        string idempotencyKey,
        CreateEnergyOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopedKey = $"{userId}:{idempotencyKey}";

        var existingResult =
            await _idempotencyService
                .GetResultAsync<CreateEnergyOfferRequest, CreateEnergyOfferResult>(
                    scopedKey,
                    Operation,
                    request,
                    cancellationToken);

        if (existingResult is not null)
        {
            return existingResult;
        }

        CreateEnergyOfferResult? result = null;

        try
        {
            await _transactionManager.ExecuteAsync(
                async transactionCancellationToken =>
                {
                    result = await _sender.Send(
                        CreateEnergyOfferCommand.FromRequest(
                            userId,
                            request),
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
                    .GetResultAsync<CreateEnergyOfferRequest, CreateEnergyOfferResult>(
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
                "Energy offer creation did not return a result.");
    }
}