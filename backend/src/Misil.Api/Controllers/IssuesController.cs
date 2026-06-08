using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Misil.Api.Hubs;
using Misil.Api.Services;
using Misil.Application.Common.DTOs;
using Misil.Application.Common.Interfaces;
using Misil.Application.Common.Services;
using Misil.Application.Issues;
using Misil.Application.Issues.Commands;
using Misil.Application.Issues.Queries;
using Misil.Domain.Enums;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/issues")]
public class IssuesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly IPermissionService _perm;
    private readonly IHubContext<ProjectHub> _hub;

    public IssuesController(IMediator mediator, ICurrentUserService currentUser, IAuditService audit, IPermissionService perm, IHubContext<ProjectHub> hub)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _audit = audit;
        _perm = perm;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<IssueDto>>> GetAll(
        Guid projectId,
        [FromQuery] IssueStatus? status,
        [FromQuery] Guid? sprintId,
        [FromQuery] Guid? assigneeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetIssuesQuery(projectId, status, sprintId, assigneeId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IssueDetailDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetIssueByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IssueDto>> Create(Guid projectId, CreateIssueCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "create_issues")) return Forbid();
        var cmd = command with { ProjectId = projectId, ReporterId = _currentUser.UserId.Value };
        var result = await _mediator.Send(cmd);
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Issue, result.Id.ToString(), "created", newValue: result.Title);
        await _hub.Clients.Group($"project_{projectId}").SendAsync("IssueCreated", result);
        return CreatedAtAction(nameof(GetById), new { projectId, id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<IssueDto>> Update(Guid projectId, Guid id, UpdateIssueCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "create_issues")) return Forbid();
        var cmd = command with { Id = id };
        var result = await _mediator.Send(cmd);
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Issue, id.ToString(), "updated", newValue: result.Title);
        await _hub.Clients.Group($"project_{projectId}").SendAsync("IssueUpdated", result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "create_issues")) return Forbid();
        await _mediator.Send(new DeleteIssueCommand(id));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Issue, id.ToString(), "deleted");
        await _hub.Clients.Group($"project_{projectId}").SendAsync("IssueDeleted", id);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid projectId, Guid id, [FromBody] IssueStatus status)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        await _mediator.Send(new UpdateIssueStatusCommand(id, status));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Issue, id.ToString(), "status_changed", newValue: status.ToString());
        await _hub.Clients.Group($"project_{projectId}").SendAsync("IssueStatusChanged", id, status);
        return NoContent();
    }

    public record AssignRequest(Guid? AssigneeId, Guid? AssigneeTeamId);

    [HttpPatch("{id}/assign")]
    public async Task<IActionResult> Assign(Guid projectId, Guid id, [FromBody] AssignRequest request)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "create_issues")) return Forbid();
        await _mediator.Send(new AssignIssueCommand(id, request.AssigneeId, request.AssigneeTeamId));
        var who = request.AssigneeTeamId?.ToString() ?? request.AssigneeId?.ToString() ?? "unassigned";
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Issue, id.ToString(), "assigned", newValue: who);
        await _hub.Clients.Group($"project_{projectId}").SendAsync("IssueAssigned", id, request.AssigneeId, request.AssigneeTeamId);
        return NoContent();
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid projectId, Guid id, AddCommentCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var cmd = command with { IssueId = id, UserId = _currentUser.UserId.Value };
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { projectId, id }, result);
    }
}
