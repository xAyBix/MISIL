using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data;
public class MisilDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public MisilDbContext(DbContextOptions<MisilDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueComment> IssueComments => Set<IssueComment>();
    public DbSet<IssueAttachment> IssueAttachments => Set<IssueAttachment>();
    public DbSet<IssueRelation> IssueRelations => Set<IssueRelation>();
    public DbSet<IssueTag> IssueTags => Set<IssueTag>();
    public DbSet<IssueTagMapping> IssueTagMappings => Set<IssueTagMapping>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<SprintIssue> SprintIssues => Set<SprintIssue>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<IssueDependency> IssueDependencies => Set<IssueDependency>();
    public DbSet<ChatChannel> ChatChannels => Set<ChatChannel>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatMessageRead> ChatMessageReads => Set<ChatMessageRead>();
    public DbSet<Role> ProjectRoles => Set<Role>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ProjectInvitation> Invitations => Set<ProjectInvitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatMessageRead>()
            .HasKey(cmr => new { cmr.UserId, cmr.MessageId });

        builder.Entity<IssueTagMapping>()
            .HasKey(itm => new { itm.IssueId, itm.TagId });

        builder.ApplyConfigurationsFromAssembly(typeof(MisilDbContext).Assembly);
    }
}
