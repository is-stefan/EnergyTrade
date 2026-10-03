using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Abstractions.Security;
using EnergyTrade.Application.Auth.Login;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Tests.Auth.Login;

public class LoginServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCredentialsAreValid_ReturnsToken()
    {
        // Arrange
        var user = new User(
            "user@test.com",
            "hashed-password");

        var userRepository = new FakeUserRepository(user);
        var passwordHasher = new FakePasswordHasher(true);
        var jwtTokenGenerator = new FakeJwtTokenGenerator();

        var service = new LoginService(
            userRepository,
            passwordHasher,
            jwtTokenGenerator);

        var request = new LoginRequest(
            "user@test.com",
            "correct-password");

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal("test-token", result.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ThrowsUnauthorizedException()
    {
        // Arrange
        var userRepository = new FakeUserRepository(null);
        var passwordHasher = new FakePasswordHasher(true);
        var jwtTokenGenerator = new FakeJwtTokenGenerator();

        var service = new LoginService(
            userRepository,
            passwordHasher,
            jwtTokenGenerator);

        var request = new LoginRequest(
            "missing@test.com",
            "password");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordIsInvalid_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User(
            "user@test.com",
            "hashed-password");

        var userRepository = new FakeUserRepository(user);
        var passwordHasher = new FakePasswordHasher(false);
        var jwtTokenGenerator = new FakeJwtTokenGenerator();

        var service = new LoginService(
            userRepository,
            passwordHasher,
            jwtTokenGenerator);

        var request = new LoginRequest(
            "user@test.com",
            "wrong-password");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.ExecuteAsync(request));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailIsEmpty_ThrowsArgumentException()
    {
        var service = new LoginService(
            new FakeUserRepository(null),
            new FakePasswordHasher(true),
            new FakeJwtTokenGenerator());

        var request = new LoginRequest(
            "",
            "password");

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordIsEmpty_ThrowsArgumentException()
    {
        var service = new LoginService(
            new FakeUserRepository(null),
            new FakePasswordHasher(true),
            new FakeJwtTokenGenerator());

        var request = new LoginRequest(
            "user@test.com",
            "");

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(request));
    }

    private sealed class FakeUserRepository
        : IUserRepository
    {
        private readonly User? _user;

        public FakeUserRepository(User? user)
        {
            _user = user;
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_user?.Id == id)
            {
                return Task.FromResult<User?>(_user);
            }

            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            if (_user?.Email == email)
            {
                return Task.FromResult<User?>(_user);
            }

            return Task.FromResult<User?>(null);
        }

        public Task<bool> EmailExistsAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _user?.Email == email);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher
        : IPasswordHasher
    {
        private readonly bool _passwordValid;

        public FakePasswordHasher(bool passwordValid)
        {
            _passwordValid = passwordValid;
        }

        public string Hash(string password)
        {
            return "hashed-password";
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            return _passwordValid;
        }
    }

    private sealed class FakeJwtTokenGenerator
        : IJwtTokenGenerator
    {
        public string Generate(
            Guid userId,
            string email)
        {
            return "test-token";
        }
    }
}