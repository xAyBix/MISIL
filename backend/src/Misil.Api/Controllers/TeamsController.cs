using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Misil.Api.Services;
using Misil.Application.Common.Interfaces;
using Misil.Application.Common.Services;
using Misil.Application.Teams;
using Misil.Application.Teams.Commands;
using Misil.Application.Teams.Queries;
using Misil.Domain.Enums;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/teams")]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly IPermissionService _perm;

    public TeamsController(IMediator mediator, ICurrentUserService currentUser, IAuditService audit, IPermissionService perm)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _audit = audit;
        _perm = perm;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeamDto>>> GetAll(Guid projectId)
    {
        var result = await _mediator.Send(new GetTeamsQuery(projectId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> Create(Guid projectId, CreateTeamCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_teams")) return Forbid();
        var cmd = command with { ProjectId = projectId };
        var result = await _mediator.Send(cmd);
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Team, result.Id.ToString(), "created", newValue: result.Name);
        return CreatedAtAction(nameof(GetAll), new { projectId }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TeamDto>> Update(Guid projectId, Guid id, UpdateTeamCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_teams")) return Forbid();
        var cmd = command with { Id = id };
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_teams")) return Forbid();
        await _mediator.Send(new DeleteTeamCommand(id));
        return NoContent();
    }

    [HttpGet("{teamId}/members")]
    public async Task<ActionResult<List<TeamMemberDetailDto>>> GetMembers(Guid teamId)
    {
        var result = await _mediator.Send(new GetTeamMembersQuery(teamId));
        return Ok(result);
    }

    [HttpPost("{teamId}/members/{userId}")]
    public async Task<IActionResult> AddMember(Guid projectId, Guid teamId, Guid userId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_teams")) return Forbid();
        await _mediator.Send(new AddTeamMemberCommand(teamId, userId));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Team, teamId.ToString(), "member_added", newValue: userId.ToString());
        return NoContent();
    }

    [HttpDelete("{teamId}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid teamId, Guid userId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_teams")) return Forbid();
        await _mediator.Send(new RemoveTeamMemberCommand(teamId, userId));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Team, teamId.ToString(), "member_removed", oldValue: userId.ToString());
        return NoContent();
    }
}
