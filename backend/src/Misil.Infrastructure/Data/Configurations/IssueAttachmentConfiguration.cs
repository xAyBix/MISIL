using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class IssueAttachmentConfiguration : IEntityTypeConfiguration<IssueAttachment>
{
    public void Configure(EntityTypeBuilder<IssueAttachment> builder)
    {
        builder.ToTable("IssueAttachments");

        builder.Property(ia => ia.FileName).IsRequired().HasMaxLength(500);
        builder.Property(ia => ia.FileUrl).IsRequired().HasMaxLength(1000);
        builder.Property(ia => ia.Size);
        builder.Property(ia => ia.UploadedAt).IsRequired();

        builder.HasOne(ia => ia.Issue)
            .WithMany(i => i.Attachments)
            .HasForeignKey(ia => ia.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ia => ia.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ia => ia.IssueId);
    }
}
