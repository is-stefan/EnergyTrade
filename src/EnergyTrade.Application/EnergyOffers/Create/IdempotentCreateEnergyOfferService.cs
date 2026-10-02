using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Application.Idempotency;

namespace EnergyTrade.Application.EnergyOffers.Create;

public sealed class IdempotentCreateEnergyOfferService
{
    private const string Operation = "CreateEnergyOffer";

    private readonly CreateEnergyOfferService _createEnergyOfferService;
    private readonly IdempotencyService _idempotencyService;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ITransactionManager _transactionManager;

    public IdempotentCreateEnergyOfferService(
        CreateEnergyOfferService createEnergyOfferService,
        IdempotencyService idempotencyService,
        IIdempotencyRepository idempotencyRepository,
        ITransactionManager transactionManager)
    {
        _createEnergyOfferService = createEnergyOfferService;
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
                    result =
                        await _createEnergyOfferService.ExecuteAsync(
                            userId,
                            request,
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