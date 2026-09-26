using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Infrastructure.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace EnergyTrade.Infrastructure.Persistence.Transactions;

public sealed class EfTransactionManager
    : ITransactionManager
{
    private readonly EnergyTradeDbContext _dbContext;

    public EfTransactionManager(
        EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await operation(cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw new ConcurrencyConflictException(
                "The data was modified by another operation.",
                exception);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is OracleException oracleException
                && oracleException.Number == 1)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw new ConcurrencyConflictException(
                "A concurrent operation modified the same trading data.",
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}