using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Misil.Application.Common.Interfaces;
using Misil.Domain.Entities;
using Misil.Infrastructure.Data;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly MisilDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(MisilDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetMyNotifications()
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var notifications = await _context.Notifications
            .Where(n => n.UserId == _currentUser.UserId.Value)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .AsNoTracking()
            .ToListAsync();
        return Ok(notifications);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();
        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (_currentUser.UserId is null) return Unauthorized();
        await _context.Notifications
            .Where(n => n.UserId == _currentUser.UserId.Value && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return NoContent();
    }
}
