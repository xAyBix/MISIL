using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;
using Misil.Domain.Enums;

namespace Misil.Infrastructure.Data.Configurations;
public class IssueDependencyConfiguration : IEntityTypeConfiguration<IssueDependency>
{
    public void Configure(EntityTypeBuilder<IssueDependency> builder)
    {
        builder.ToTable("IssueDependencies");

        builder.Property(id => id.DependencyType).IsRequired().HasConversion<string>().HasMaxLength(10);

        builder.HasOne(d => d.Issue)
            .WithMany(i => i.Dependencies)
            .HasForeignKey(d => d.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.DependsOnIssue)
            .WithMany()
            .HasForeignKey(d => d.DependsOnIssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.IssueId);
        builder.HasIndex(d => d.DependsOnIssueId);
    }
}
