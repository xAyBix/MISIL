using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;
public class RoleRepository : IRoleRepository
{
    private readonly MisilDbContext _context;
    public RoleRepository(MisilDbContext context) => _context = context;

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ProjectRoles.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _context.ProjectRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
        => await _context.ProjectRoles.AsNoTracking().ToListAsync(ct);

    public async Task<Role> AddAsync(Role role, CancellationToken ct = default)
    {
        await _context.ProjectRoles.AddAsync(role, ct);
        await _context.SaveChangesAsync(ct);
        return role;
    }

    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        _context.ProjectRoles.Update(role);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _context.ProjectRoles.FindAsync([id], ct);
        if (role != null)
        {
            _context.ProjectRoles.Remove(role);
            await _context.SaveChangesAsync(ct);
        }
    }
}
