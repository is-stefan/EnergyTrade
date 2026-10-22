using EnergyTrade.Application;
using EnergyTrade.Application.Orders.Create;
using EnergyTrade.Domain.Enums;
using EnergyTrade.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyTrade.IntegrationTests;

public class CreateOrderDependencyInjectionTests
{
    [Fact]
    public async Task RegisteredSender_DispatchesCreateOrderCommandToHandler()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:OracleDatabase"] =
                    "User Id=unused;Password=unused;Data Source=localhost:1521/unused"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);

        await using var provider =
            services.BuildServiceProvider();

        await using var scope =
            provider.CreateAsyncScope();

        Assert.IsType<CreateOrderCommandHandler>(
            scope.ServiceProvider
                .GetRequiredService<
                    IRequestHandler<
                        CreateOrderCommand,
                        CreateOrderResult>>());

        Assert.NotNull(
            scope.ServiceProvider
                .GetRequiredService<
                    IdempotentCreateOrderService>());

        var start =
            DateTimeOffset.Now.AddDays(1);

        var request =
            new CreateOrderRequest(
                Guid.NewGuid(),
                EnergyType.Solar,
                100m,
                85m,
                Currency.EUR,
                start,
                start.AddDays(1));

        var command =
            CreateOrderCommand.FromRequest(
                Guid.Empty,
                request);

        var sender =
            scope.ServiceProvider
                .GetRequiredService<ISender>();

        // Validation pipeline executes before the handler
        // and before any database access.
        var exception =
            await Assert.ThrowsAsync<ValidationException>(
                () => sender.Send(command));

        Assert.Contains(
            exception.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateOrderCommand.UserId));

        Assert.Equal(
            request.DeliveryStart,
            command.DeliveryStart);

        Assert.Equal(
            request.DeliveryStart.Offset,
            command.DeliveryStart.Offset);

        Assert.Equal(
            request.DeliveryEnd,
            command.DeliveryEnd);
    }
}