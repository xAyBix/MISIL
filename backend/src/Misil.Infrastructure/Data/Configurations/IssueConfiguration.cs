using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;
using Misil.Domain.Enums;

namespace Misil.Infrastructure.Data.Configurations;
public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues");

        builder.Property(i => i.Title).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Description).HasMaxLength(4000);
        builder.Property(i => i.IssueType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Priority).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.UpdatedAt).IsRequired();

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Issues)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Reporter)
            .WithMany(u => u.ReportedIssues)
            .HasForeignKey(i => i.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Assignee)
            .WithMany(u => u.AssignedIssues)
            .HasForeignKey(i => i.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ParentIssue)
            .WithMany(i => i.Subtasks)
            .HasForeignKey(i => i.ParentIssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.AssigneeId);
        builder.HasIndex(i => i.ReporterId);
        builder.HasIndex(i => i.Status);
    }
}
