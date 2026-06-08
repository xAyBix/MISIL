using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Misil.Api.Services;
using Misil.Application.Common.Interfaces;
using Misil.Application.Common.Services;
using Misil.Application.Projects;
using Misil.Application.Projects.Commands;
using Misil.Application.Projects.Queries;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Infrastructure.Data;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly MisilDbContext _context;
    private readonly IPermissionService _perm;

    public ProjectsController(IMediator mediator, ICurrentUserService currentUser, IAuditService audit, MisilDbContext context, IPermissionService perm)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _audit = audit;
        _context = context;
        _perm = perm;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll()
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var result = await _mediator.Send(new GetProjectsQuery(_currentUser.UserId.Value));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var result = await _mediator.Send(new GetProjectByIdQuery(id, _currentUser.UserId.Value));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var cmd = command with { LeadUserId = _currentUser.UserId.Value };
        var result = await _mediator.Send(cmd);
        await _audit.LogAsync(result.Id, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Project, result.Id.ToString(), "created", newValue: result.Name);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, UpdateProjectCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(id, "edit_project")) return Forbid();
        var cmd = command with { Id = id };
        var result = await _mediator.Send(cmd);
        await _audit.LogAsync(id, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Project, id.ToString(), "updated", newValue: result.Name);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(id, "edit_project")) return Forbid();
        await _mediator.Send(new DeleteProjectCommand(id));
        await _audit.LogAsync(id, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Project, id.ToString(), "deleted");
        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<ProjectMemberDto>>> GetMembers(Guid id)
    {
        var result = await _mediator.Send(new GetProjectMembersQuery(id));
        return Ok(result);
    }

    public record AddMemberRequest(string Email, Guid RoleId);

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(Guid id, AddMemberRequest request)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(id, "invite_members")) return Forbid();

        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null) return BadRequest("User not found");

        var existing = _context.ProjectMembers.Any(m => m.ProjectId == id && m.UserId == user.Id);
        if (existing) return BadRequest("User is already a member");

        var pending = _context.Invitations.Any(i => i.ProjectId == id && i.InvitedUserId == user.Id && i.Status == "Pending");
        if (pending) return BadRequest("User already has a pending invitation");

        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound("Project not found");

        var invitation = new ProjectInvitation
        {
            Id = Guid.NewGuid(),
            ProjectId = id,
            InvitedUserId = user.Id,
            InvitedByUserId = _currentUser.UserId.Value,
            RoleId = request.RoleId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.Invitations.Add(invitation);

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ProjectId = id,
            Message = $"You've been invited to project \"{project.Name}\"",
            Action = "invitation",
            ActorId = _currentUser.UserId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        await _audit.LogAsync(id, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Invitation, invitation.Id.ToString(), "invited", newValue: user.Email);
        return Ok(invitation);
    }

    [HttpPatch("{id}/members/{userId}/role")]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid userId, [FromBody] Guid roleId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(id, "invite_members")) return Forbid();
        await _mediator.Send(new UpdateProjectMemberRoleCommand(id, userId, roleId));
        await _audit.LogAsync(id, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Member, userId.ToString(), "role_changed", newValue: roleId.ToString());
        return NoContent();
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(id, "remove_members")) return Forbid();
        await _mediator.Send(new RemoveProjectMemberCommand(id, userId));
        await _audit.LogAsync(id, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Member, userId.ToString(), "removed");
        return NoContent();
    }

    [HttpGet("{id}/my-permissions")]
    public async Task<ActionResult<List<string>>> MyPermissions(Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var member = await _context.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == _currentUser.UserId.Value);

        if (member == null) return Ok(new List<string>());

        var role = await _context.ProjectRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == member.RoleId);
        if (role == null) return Ok(new List<string>());

        if (role.Permissions == "all")
            return Ok(AllPermissions.List);

        return Ok(role.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
    }
}
