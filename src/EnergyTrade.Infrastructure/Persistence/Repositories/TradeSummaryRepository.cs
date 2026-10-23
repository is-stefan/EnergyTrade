using System.Data;
using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Trades.Summary;
using EnergyTrade.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace EnergyTrade.Infrastructure.Persistence.Repositories;

public sealed class TradeSummaryRepository
    : ITradeSummaryRepository
{
    private readonly EnergyTradeDbContext _dbContext;

    public TradeSummaryRepository(
        EnergyTradeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TradeSummaryResult> GetUserTradeSummaryAsync(
        Guid userId,
        Currency currency,
        EnergyType? energyType = null,
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
                "GET_USER_TRADE_SUMMARY";

            command.CommandType =
                CommandType.StoredProcedure;

            command.BindByName = true;

            command.Parameters.Add(
                new OracleParameter(
                    "p_user_id",
                    OracleDbType.Raw,
                    userId.ToByteArray(),
                    ParameterDirection.Input));

            command.Parameters.Add(
                new OracleParameter(
                    "p_currency",
                    OracleDbType.Int32,
                    (int)currency,
                    ParameterDirection.Input));

            command.Parameters.Add(
                new OracleParameter(
                    "p_energy_type",
                    OracleDbType.Int32,
                    energyType.HasValue
                        ? (int)energyType.Value
                        : DBNull.Value,
                    ParameterDirection.Input));

            var tradeCountParameter =
                new OracleParameter(
                    "p_trade_count",
                    OracleDbType.Decimal,
                    ParameterDirection.Output);

            var totalMWhParameter =
                new OracleParameter(
                    "p_total_mwh",
                    OracleDbType.Decimal,
                    ParameterDirection.Output);

            var averagePriceParameter =
                new OracleParameter(
                    "p_average_price",
                    OracleDbType.Decimal,
                    ParameterDirection.Output);

            var totalValueParameter =
                new OracleParameter(
                    "p_total_value",
                    OracleDbType.Decimal,
                    ParameterDirection.Output);

            command.Parameters.Add(tradeCountParameter);
            command.Parameters.Add(totalMWhParameter);
            command.Parameters.Add(averagePriceParameter);
            command.Parameters.Add(totalValueParameter);

            await command.ExecuteNonQueryAsync(
                cancellationToken);

            var tradeCount =
                GetInt32(tradeCountParameter);

            var totalMWh =
                GetDecimal(totalMWhParameter);

            var averagePrice =
                GetDecimal(averagePriceParameter);

            var totalValue =
                GetDecimal(totalValueParameter);

            return new TradeSummaryResult(
                tradeCount,
                totalMWh,
                averagePrice,
                totalValue);
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static int GetInt32(
        OracleParameter parameter)
    {
        if (parameter.Value is null ||
            parameter.Value == DBNull.Value)
        {
            return 0;
        }

        var value =
            (OracleDecimal)parameter.Value;

        return value.ToInt32();
    }

    private static decimal GetDecimal(
        OracleParameter parameter)
    {
        if (parameter.Value is null ||
            parameter.Value == DBNull.Value)
        {
            return 0m;
        }

        var value =
            (OracleDecimal)parameter.Value;

        return value.Value;
    }
}