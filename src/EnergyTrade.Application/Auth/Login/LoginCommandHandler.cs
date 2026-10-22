using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Abstractions.Security;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Auth.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> Handle(
        LoginCommand command,
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

        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var passwordValid =
            _passwordHasher.Verify(
                command.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var token = _jwtTokenGenerator.Generate(
            user.Id,
            user.Email);

        return new LoginResult(
            user.Id,
            user.Email,
            token);
    }
}
