using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class IssueCommentConfiguration : IEntityTypeConfiguration<IssueComment>
{
    public void Configure(EntityTypeBuilder<IssueComment> builder)
    {
        builder.ToTable("IssueComments");

        builder.Property(ic => ic.Content).IsRequired().HasMaxLength(4000);
        builder.Property(ic => ic.CreatedAt).IsRequired();
        builder.Property(ic => ic.UpdatedAt).IsRequired();

        builder.HasOne(ic => ic.Issue)
            .WithMany(i => i.Comments)
            .HasForeignKey(ic => ic.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ic => ic.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(ic => ic.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ic => ic.IssueId);
        builder.HasIndex(ic => ic.UserId);
    }
}
