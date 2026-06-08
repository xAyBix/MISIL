using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Misil.Application.Common.Interfaces;
using Misil.Application.Common.Services;
using Misil.Application.Projects.Commands;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Infrastructure.Data;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly MisilDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly IMediator _mediator;

    public InvitationsController(MisilDbContext context, ICurrentUserService currentUser, IAuditService audit, IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
        _mediator = mediator;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<ProjectInvitation>>> GetPending()
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var invitations = await _context.Invitations
            .Include(i => i.Project)
            .Include(i => i.InvitedByUser)
            .Where(i => i.InvitedUserId == _currentUser.UserId.Value && i.Status == "Pending")
            .OrderByDescending(i => i.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
        return Ok(invitations);
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var invitation = await _context.Invitations
            .Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Id == id && i.InvitedUserId == _currentUser.UserId.Value);
        if (invitation == null) return NotFound();
        if (invitation.Status != "Pending") return BadRequest("Invitation is no longer pending");

        invitation.Status = "Accepted";
        invitation.RespondedAt = DateTime.UtcNow;

        await _mediator.Send(new AddProjectMemberCommand(invitation.ProjectId, _currentUser.UserId.Value, invitation.RoleId));
        await _context.SaveChangesAsync();
        await _audit.LogAsync(invitation.ProjectId, _currentUser.UserId.Value, _currentUser.UserName ?? "", EntityType.Member, _currentUser.UserId.Value.ToString(), "joined");
        return NoContent();
    }

    [HttpPost("{id}/deny")]
    public async Task<IActionResult> Deny(Guid id)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Id == id && i.InvitedUserId == _currentUser.UserId.Value);
        if (invitation == null) return NotFound();
        if (invitation.Status != "Pending") return BadRequest("Invitation is no longer pending");

        invitation.Status = "Denied";
        invitation.RespondedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
