using Microsoft.EntityFrameworkCore;
using Misil.Application.Common.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Api.Services;

public class PermissionService : IPermissionService
{
    private readonly MisilDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PermissionService(MisilDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> HasPermissionAsync(Guid projectId, string permission)
    {
        if (_currentUser.UserId is null) return false;

        var member = await _context.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == _currentUser.UserId.Value);

        if (member == null) return false;

        var role = await _context.ProjectRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == member.RoleId);
        if (role == null) return false;

        if (role.Permissions == "all") return true;

        return role.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(permission);
    }

    public async Task<bool> IsOwnerAsync(Guid projectId)
    {
        if (_currentUser.UserId is null) return false;

        var member = await _context.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == _currentUser.UserId.Value);

        if (member == null) return false;

        var role = await _context.ProjectRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == member.RoleId);
        return role?.Name == "Owner";
    }
}
