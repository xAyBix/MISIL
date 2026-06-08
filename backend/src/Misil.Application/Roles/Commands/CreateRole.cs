using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Roles.Commands;
public record CreateRoleCommand(string Name, string? Description, string? Permissions) : IRequest<Role>;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Role>
{
    private readonly IRoleRepository _roleRepo;
    public CreateRoleCommandHandler(IRoleRepository roleRepo) => _roleRepo = roleRepo;

    public async Task<Role> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Permissions = request.Permissions ?? ""
        };
        return await _roleRepo.AddAsync(role, ct);
    }
}
