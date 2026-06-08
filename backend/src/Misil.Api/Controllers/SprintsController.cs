using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Misil.Api.Services;
using Misil.Application.Common.Interfaces;
using Misil.Application.Common.Services;
using Misil.Application.Sprints;
using Misil.Application.Sprints.Commands;
using Misil.Application.Sprints.Queries;
using Misil.Domain.Enums;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/sprints")]
public class SprintsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly IPermissionService _perm;

    public SprintsController(IMediator mediator, ICurrentUserService currentUser, IAuditService audit, IPermissionService perm)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _audit = audit;
        _perm = perm;
    }

    [HttpGet]
    public async Task<ActionResult<List<SprintDto>>> GetAll(Guid projectId)
    {
        var result = await _mediator.Send(new GetSprintsQuery(projectId));
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<SprintDto?>> GetActive(Guid projectId)
    {
        var result = await _mediator.Send(new GetActiveSprintQuery(projectId));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SprintDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSprintByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SprintDto>> Create(Guid projectId, CreateSprintCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_sprints")) return Forbid();
        var cmd = command with { ProjectId = projectId };
        var result = await _mediator.Send(cmd);
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Sprint, result.Id.ToString(), "created", newValue: result.Name);
        return CreatedAtAction(nameof(GetById), new { projectId, id = result.Id }, result);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid projectId, Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_sprints")) return Forbid();
        await _mediator.Send(new StartSprintCommand(id));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Sprint, id.ToString(), "started");
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid projectId, Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_sprints")) return Forbid();
        await _mediator.Send(new CompleteSprintCommand(id));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Sprint, id.ToString(), "completed");
        return NoContent();
    }

    [HttpPost("{sprintId}/issues/{issueId}")]
    public async Task<IActionResult> AddIssue(Guid projectId, Guid sprintId, Guid issueId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_sprints")) return Forbid();
        await _mediator.Send(new AddIssueToSprintCommand(sprintId, issueId));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Issue, issueId.ToString(), "added_to_sprint", newValue: sprintId.ToString());
        return NoContent();
    }

    [HttpDelete("{sprintId}/issues/{issueId}")]
    public async Task<IActionResult> RemoveIssue(Guid projectId, Guid sprintId, Guid issueId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_sprints")) return Forbid();
        await _mediator.Send(new RemoveIssueFromSprintCommand(sprintId, issueId));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Issue, issueId.ToString(), "removed_from_sprint", oldValue: sprintId.ToString());
        return NoContent();
    }
}
