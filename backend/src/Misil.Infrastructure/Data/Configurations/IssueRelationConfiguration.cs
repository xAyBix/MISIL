using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;
using Misil.Domain.Enums;

namespace Misil.Infrastructure.Data.Configurations;
public class IssueRelationConfiguration : IEntityTypeConfiguration<IssueRelation>
{
    public void Configure(EntityTypeBuilder<IssueRelation> builder)
    {
        builder.ToTable("IssueRelations");

        builder.Property(ir => ir.RelationType).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasOne(ir => ir.Issue)
            .WithMany(i => i.RelatedIssues)
            .HasForeignKey(ir => ir.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Issue>()
            .WithMany()
            .HasForeignKey(ir => ir.RelatedIssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ir => ir.IssueId);
        builder.HasIndex(ir => ir.RelatedIssueId);
    }
}
