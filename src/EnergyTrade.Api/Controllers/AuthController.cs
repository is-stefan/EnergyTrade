using EnergyTrade.Application.Auth.Register;
using Microsoft.AspNetCore.Mvc;
using EnergyTrade.Application.Auth.Login;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterService _registerService;

    private readonly LoginService _loginService;

    public AuthController(
        RegisterService registerService,
        LoginService loginService)
    {
        _registerService = registerService;
        _loginService = loginService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResult>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registerService.ExecuteAsync(
            request,
            cancellationToken);

        return Created(
            $"/api/users/{result.Id}",
            result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _loginService.ExecuteAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}