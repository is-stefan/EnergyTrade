using EnergyTrade.Application.Auth.Login;
using EnergyTrade.Application.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnergyTrade.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResult>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            RegisterCommand.FromRequest(request),
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
        var result = await _sender.Send(
            LoginCommand.FromRequest(request),
            cancellationToken);

        return Ok(result);
    }
}
