using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Abstractions.Security;
using EnergyTrade.Application.Auth.Register;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Tests.Auth.Register;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_CreatesUser()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();

        var handler = new RegisterCommandHandler(
            userRepository,
            passwordHasher);

        var command = new RegisterCommand(
            "Test@EnergyTrade.com",
            "Password123!");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(
            "test@energytrade.com",
            result.Email);

        Assert.NotNull(userRepository.AddedUser);

        Assert.Equal(
            "test@energytrade.com",
            userRepository.AddedUser.Email);

        Assert.Equal(
            "HASHED_Password123!",
            userRepository.AddedUser.PasswordHash);

        Assert.Equal(1, userRepository.AddCallCount);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
        Assert.Equal(1, passwordHasher.HashCallCount);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var userRepository = new FakeUserRepository
        {
            EmailExists = true
        };

        var passwordHasher = new FakePasswordHasher();

        var handler = new RegisterCommandHandler(
            userRepository,
            passwordHasher);

        var command = new RegisterCommand(
            "test@energytrade.com",
            "Password123!");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Null(userRepository.AddedUser);
        Assert.Equal(0, userRepository.AddCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
        Assert.Equal(0, passwordHasher.HashCallCount);
    }

    [Fact]
    public async Task Handle_WithEmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();

        var handler = new RegisterCommandHandler(
            userRepository,
            passwordHasher);

        var command = new RegisterCommand(
            "",
            "Password123!");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, userRepository.AddCallCount);
        Assert.Equal(0, passwordHasher.HashCallCount);
    }

    [Fact]
    public async Task Handle_WithEmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();

        var handler = new RegisterCommandHandler(
            userRepository,
            passwordHasher);

        var command = new RegisterCommand(
            "test@energytrade.com",
            "");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(0, userRepository.AddCallCount);
        Assert.Equal(0, passwordHasher.HashCallCount);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? AddedUser { get; private set; }

        public bool EmailExists { get; init; }

        public int AddCallCount { get; private set; }

        public int SaveChangesCallCount { get; private set; }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            AddedUser = user;
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<bool> EmailExistsAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmailExists);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public int HashCallCount { get; private set; }

        public string Hash(string password)
        {
            HashCallCount++;

            return $"HASHED_{password}";
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            return passwordHash == $"HASHED_{password}";
        }
    }
}
