using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class ChatMessageReadConfiguration : IEntityTypeConfiguration<ChatMessageRead>
{
    public void Configure(EntityTypeBuilder<ChatMessageRead> builder)
    {
        builder.ToTable("ChatMessageReads");

        builder.Property(cmr => cmr.ReadAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(cmr => cmr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChatMessage>()
            .WithMany()
            .HasForeignKey(cmr => cmr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cmr => cmr.MessageId);
    }
}
