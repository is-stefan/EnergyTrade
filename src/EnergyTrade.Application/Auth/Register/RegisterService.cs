using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Abstractions.Security;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Auth.Register;

public sealed class RegisterService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> ExecuteAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "Password cannot be empty.",
                nameof(request.Password));
        }

        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var emailExists =
            await _userRepository.EmailExistsAsync(
                normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var passwordHash =
            _passwordHasher.Hash(request.Password);

        var user = new User(
            normalizedEmail,
            passwordHash);

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return new RegisterResult(
            user.Id,
            user.Email,
            user.CreatedAt);
    }
}