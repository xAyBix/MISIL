using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Misil.Api.Hubs;
using Misil.Api.Services;
using Misil.Application.Chat;
using Misil.Application.Chat.Commands;
using Misil.Application.Chat.Queries;
using Misil.Application.Common.Interfaces;
using Misil.Application.Common.Services;
using Misil.Domain.Enums;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/chat")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly IPermissionService _perm;
    private readonly IHubContext<ProjectHub> _hub;

    public ChatController(IMediator mediator, ICurrentUserService currentUser, IAuditService audit, IPermissionService perm, IHubContext<ProjectHub> hub)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _audit = audit;
        _perm = perm;
        _hub = hub;
    }

    [HttpGet("channels")]
    public async Task<ActionResult<List<ChannelDto>>> GetChannels(Guid projectId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var result = await _mediator.Send(new GetChannelsQuery(projectId, _currentUser.UserId.Value));
        return Ok(result);
    }

    [HttpPost("channels")]
    public async Task<ActionResult<ChannelDto>> CreateChannel(Guid projectId, CreateChannelCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_channels")) return Forbid();
        var cmd = command with { ProjectId = projectId };
        var result = await _mediator.Send(cmd);
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.ChatMessage, result.Id.ToString(), "channel_created", newValue: result.Name);
        await _hub.Clients.Group($"project_{projectId}").SendAsync("ChannelCreated", result);
        return CreatedAtAction(nameof(GetChannels), new { projectId }, result);
    }

    [HttpGet("channels/{channelId}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(Guid channelId, [FromQuery] int limit = 50, [FromQuery] Guid? before = null)
    {
        var result = await _mediator.Send(new GetMessagesQuery(channelId, limit, before));
        return Ok(result);
    }

    [HttpPost("channels/{channelId}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(Guid projectId, Guid channelId, SendMessageCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var cmd = command with { ChannelId = channelId, UserId = _currentUser.UserId.Value };
        var result = await _mediator.Send(cmd);
        await _hub.Clients.Group($"project_{projectId}").SendAsync("MessageSent", result);
        return CreatedAtAction(nameof(GetMessages), new { projectId, channelId }, result);
    }

    [HttpPost("messages/{messageId}/read")]
    public async Task<IActionResult> MarkRead(Guid messageId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        await _mediator.Send(new MarkMessageReadCommand(_currentUser.UserId.Value, messageId));
        return NoContent();
    }
}
