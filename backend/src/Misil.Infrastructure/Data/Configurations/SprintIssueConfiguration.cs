using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class SprintIssueConfiguration : IEntityTypeConfiguration<SprintIssue>
{
    public void Configure(EntityTypeBuilder<SprintIssue> builder)
    {
        builder.ToTable("SprintIssues");

        builder.HasOne(si => si.Sprint)
            .WithMany(s => s.SprintIssues)
            .HasForeignKey(si => si.SprintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.Issue)
            .WithMany(i => i.SprintIssues)
            .HasForeignKey(si => si.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(si => si.SprintId);
        builder.HasIndex(si => si.IssueId);
    }
}
