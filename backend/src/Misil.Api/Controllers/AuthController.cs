using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Misil.Application.Auth;
using Misil.Application.Auth.Commands;
using Misil.Application.Auth.Queries;
using Misil.Application.Common.Interfaces;

namespace Misil.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var result = await _mediator.Send(new GetCurrentUserQuery(_currentUser.UserId.Value));
        return Ok(result);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var cmd = command with { UserId = _currentUser.UserId.Value };
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }
}
