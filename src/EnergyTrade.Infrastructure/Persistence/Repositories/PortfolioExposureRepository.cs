using System.Data;
using EnergyTrade.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace EnergyTrade.Infrastructure.Persistence.Repositories;

public sealed class PortfolioExposureRepository
    : IPortfolioExposureRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public PortfolioExposureRepository(
        EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal> GetExposureAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var connection =
            (OracleConnection)_dbContext.Database.GetDbConnection();

        var shouldCloseConnection =
            connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command =
                connection.CreateCommand();

            command.CommandText =
                "GET_PORTFOLIO_EXPOSURE";

            command.CommandType =
                CommandType.StoredProcedure;

            command.BindByName = true;

            var returnParameter =
                new OracleParameter(
                    "RETURN_VALUE",
                    OracleDbType.Decimal,
                    ParameterDirection.ReturnValue);

            command.Parameters.Add(returnParameter);

            command.Parameters.Add(
                new OracleParameter(
                    "p_portfolio_id",
                    OracleDbType.Raw,
                    portfolioId.ToByteArray(),
                    ParameterDirection.Input));

            await command.ExecuteNonQueryAsync(
                cancellationToken);

            if (returnParameter.Value is null ||
                returnParameter.Value == DBNull.Value)
            {
                return 0m;
            }

            var value =
                (OracleDecimal)returnParameter.Value;

            return value.Value;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}