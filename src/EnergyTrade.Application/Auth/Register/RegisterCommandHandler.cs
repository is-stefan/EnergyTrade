using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Abstractions.Security;
using EnergyTrade.Application.Common.Exceptions;
using EnergyTrade.Domain.Entities;
using MediatR;

namespace EnergyTrade.Application.Auth.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(command.Email));
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ArgumentException(
                "Password cannot be empty.",
                nameof(command.Password));
        }

        var normalizedEmail =
            command.Email.Trim().ToLowerInvariant();

        var emailExists =
            await _userRepository.EmailExistsAsync(
                normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        var passwordHash =
            _passwordHasher.Hash(command.Password);

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
