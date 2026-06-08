using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class IssueTagMappingConfiguration : IEntityTypeConfiguration<IssueTagMapping>
{
    public void Configure(EntityTypeBuilder<IssueTagMapping> builder)
    {
        builder.ToTable("IssueTagMappings");

        builder.HasOne<Issue>()
            .WithMany()
            .HasForeignKey(itm => itm.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<IssueTag>()
            .WithMany()
            .HasForeignKey(itm => itm.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(itm => itm.TagId);
        builder.HasIndex(itm => itm.IssueId);
    }
}
