using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Roles.Commands;
public record DeleteRoleCommand(Guid Id) : IRequest;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepo;
    public DeleteRoleCommandHandler(IRoleRepository roleRepo) => _roleRepo = roleRepo;

    public async Task Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await _roleRepo.GetByIdAsync(request.Id, ct);
        if (role == null) throw new NotFoundException(nameof(Role), request.Id);
        await _roleRepo.DeleteAsync(request.Id, ct);
    }
}
