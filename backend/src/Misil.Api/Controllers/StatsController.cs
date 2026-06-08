using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Misil.Domain.Enums;
using Misil.Infrastructure.Data;

namespace Misil.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/stats")]
public class StatsController : ControllerBase
{
    private readonly MisilDbContext _context;

    public StatsController(MisilDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<ProjectStatsDto>> GetStats(Guid projectId)
    {
        var issues = _context.Issues.Where(i => i.ProjectId == projectId);
        var totalIssues = await issues.CountAsync();

        var byStatus = await issues.GroupBy(i => i.Status)
            .Select(g => new StatusCountDto(g.Key.ToString(), g.Count()))
            .ToListAsync();

        var byType = await issues.GroupBy(i => i.IssueType)
            .Select(g => new TypeCountDto(g.Key.ToString(), g.Count()))
            .ToListAsync();

        var byPriority = await issues.GroupBy(i => i.Priority)
            .Select(g => new PriorityCountDto(g.Key.ToString(), g.Count()))
            .ToListAsync();

        var totalMembers = await _context.ProjectMembers.CountAsync(pm => pm.ProjectId == projectId);
        var totalSprints = await _context.Sprints.CountAsync(s => s.ProjectId == projectId);
        var totalTeams = await _context.Teams.CountAsync(t => t.ProjectId == projectId);

        return Ok(new ProjectStatsDto(
            totalIssues, totalMembers, totalSprints, totalTeams,
            byStatus, byType, byPriority
        ));
    }
}

public record ProjectStatsDto(
    int TotalIssues, int TotalMembers, int TotalSprints, int TotalTeams,
    List<StatusCountDto> ByStatus, List<TypeCountDto> ByType, List<PriorityCountDto> ByPriority
);

public record StatusCountDto(string Status, int Count);
public record TypeCountDto(string Type, int Count);
public record PriorityCountDto(string Priority, int Count);
