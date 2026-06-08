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

        var doneIssues = issues.Where(i => i.Status == IssueStatus.Done && i.StoryPoints != null);

        var memberLeaderboard = await doneIssues
            .Where(i => i.AssigneeId != null)
            .GroupBy(i => i.AssigneeId!)
            .Select(g => new
            {
                UserId = g.Key,
                TotalStoryPoints = g.Sum(i => i.StoryPoints ?? 0),
                CompletedIssues = g.Count()
            })
            .OrderByDescending(x => x.TotalStoryPoints)
            .Take(10)
            .ToListAsync();

        var userIds = memberLeaderboard.Select(m => m.UserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? "Unknown");

        var memberBoard = memberLeaderboard.Select(m => new LeaderboardEntryDto(
            m.UserId.ToString()!, users.TryGetValue(m.UserId!.Value, out var name) ? name : "Unknown",
            m.TotalStoryPoints, m.CompletedIssues
        )).ToList();

        var teamLeaderboard = await doneIssues
            .Where(i => i.AssigneeTeamId != null)
            .GroupBy(i => i.AssigneeTeamId!)
            .Select(g => new
            {
                TeamId = g.Key,
                TotalStoryPoints = g.Sum(i => i.StoryPoints ?? 0),
                CompletedIssues = g.Count()
            })
            .OrderByDescending(x => x.TotalStoryPoints)
            .Take(10)
            .ToListAsync();

        var teamIds = teamLeaderboard.Select(t => t.TeamId).ToList();
        var teams = await _context.Teams
            .Where(t => teamIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name);

        var teamBoard = teamLeaderboard.Select(t => new LeaderboardEntryDto(
            t.TeamId.ToString()!, teams.TryGetValue(t.TeamId!.Value, out var name) ? name : "Unknown",
            t.TotalStoryPoints, t.CompletedIssues
        )).ToList();

        return Ok(new ProjectStatsDto(
            totalIssues, totalMembers, totalSprints, totalTeams,
            byStatus, byType, byPriority, memberBoard, teamBoard
        ));
    }
}

public record ProjectStatsDto(
    int TotalIssues, int TotalMembers, int TotalSprints, int TotalTeams,
    List<StatusCountDto> ByStatus, List<TypeCountDto> ByType, List<PriorityCountDto> ByPriority,
    List<LeaderboardEntryDto> MemberLeaderboard, List<LeaderboardEntryDto> TeamLeaderboard
);

public record StatusCountDto(string Status, int Count);
public record TypeCountDto(string Type, int Count);
public record PriorityCountDto(string Priority, int Count);
public record LeaderboardEntryDto(string Id, string Name, int TotalStoryPoints, int CompletedIssues);
