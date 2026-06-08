using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Misil.Application.Gantt;
using Misil.Application.Gantt.Commands;
using Misil.Application.Gantt.Queries;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/gantt")]
public class GanttController : ControllerBase
{
    private readonly IMediator _mediator;

    public GanttController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<GanttDataDto>> GetGanttData(Guid projectId)
    {
        var result = await _mediator.Send(new GetGanttDataQuery(projectId));
        return Ok(result);
    }

    [HttpPost("dependencies")]
    public async Task<ActionResult<GanttDependencyDto>> CreateDependency(CreateDependencyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetGanttData), new { projectId = "" }, result);
    }

    [HttpDelete("dependencies/{id}")]
    public async Task<IActionResult> DeleteDependency(Guid id)
    {
        await _mediator.Send(new DeleteDependencyCommand(id));
        return NoContent();
    }
}
