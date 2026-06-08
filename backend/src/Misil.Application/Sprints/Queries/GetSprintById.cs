using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Queries;
public record GetSprintByIdQuery(Guid Id) : IRequest<SprintDto>;

public class GetSprintByIdQueryHandler : IRequestHandler<GetSprintByIdQuery, SprintDto>
{
    private readonly ISprintRepository _sprintRepo;

    public GetSprintByIdQueryHandler(ISprintRepository sprintRepo) => _sprintRepo = sprintRepo;

    public async Task<SprintDto> Handle(GetSprintByIdQuery request, CancellationToken ct)
    {
        var sprint = await _sprintRepo.GetByIdAsync(request.Id, ct);
        if (sprint == null) throw new NotFoundException(nameof(Sprint), request.Id);
        return SprintDto.FromSprint(sprint);
    }
}
