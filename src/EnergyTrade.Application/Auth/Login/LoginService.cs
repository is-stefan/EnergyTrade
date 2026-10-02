using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Abstractions.Security;

namespace EnergyTrade.Application.Auth.Login;

public sealed class LoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> ExecuteAsync(
        LoginRequest request,
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

        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Invalid email or password.");
        }

        var passwordValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new InvalidOperationException(
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