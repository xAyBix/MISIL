using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Misil.Api.Services;
using Misil.Application.Common.Interfaces;
using Misil.Application.Common.Services;
using Misil.Application.Roles.Commands;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepo;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly IPermissionService _perm;

    public RolesController(IRoleRepository roleRepo, IMediator mediator, ICurrentUserService currentUser, IAuditService audit, IPermissionService perm)
    {
        _roleRepo = roleRepo;
        _mediator = mediator;
        _currentUser = currentUser;
        _audit = audit;
        _perm = perm;
    }

    [HttpGet]
    public async Task<ActionResult<List<Role>>> GetAll()
    {
        var roles = await _roleRepo.GetAllAsync();
        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<Role>> Create(Guid projectId, CreateRoleCommand command)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_roles")) return Forbid();
        var role = await _mediator.Send(command);
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Role, role.Id.ToString(), "created", newValue: role.Name);
        return CreatedAtAction(nameof(GetAll), new { projectId }, role);
    }

    [HttpPatch("{roleId}")]
    public async Task<IActionResult> Update(Guid projectId, Guid roleId, [FromBody] UpdateRoleRequest request)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_roles")) return Forbid();
        var role = await _roleRepo.GetByIdAsync(roleId);
        if (role == null) return NotFound();

        if (request.Name != null) role.Name = request.Name;
        if (request.Description != null) role.Description = request.Description;
        if (request.Permissions != null) role.Permissions = request.Permissions;

        await _roleRepo.UpdateAsync(role);
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Role, roleId.ToString(), "updated", newValue: role.Name);
        return NoContent();
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid roleId)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (!await _perm.HasPermissionAsync(projectId, "manage_roles")) return Forbid();
        var role = await _roleRepo.GetByIdAsync(roleId);
        if (role == null) return NotFound();
        if (role.Name == "Owner") return BadRequest("Cannot delete the Owner role");

        await _mediator.Send(new DeleteRoleCommand(roleId));
        await _audit.LogAsync(projectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Role, roleId.ToString(), "deleted", oldValue: role.Name);
        return NoContent();
    }
}

public record UpdateRoleRequest(string? Name, string? Description, string? Permissions);
