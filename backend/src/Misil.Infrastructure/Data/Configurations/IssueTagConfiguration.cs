using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class IssueTagConfiguration : IEntityTypeConfiguration<IssueTag>
{
    public void Configure(EntityTypeBuilder<IssueTag> builder)
    {
        builder.ToTable("IssueTags");

        builder.Property(it => it.Name).IsRequired().HasMaxLength(100);
        builder.Property(it => it.Color).HasMaxLength(20);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(it => it.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(it => it.ProjectId);
        builder.HasIndex(it => new { it.ProjectId, it.Name }).IsUnique();
    }
}
