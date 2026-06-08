using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.Property(cm => cm.Content).IsRequired().HasMaxLength(4000);
        builder.Property(cm => cm.CreatedAt).IsRequired();
        builder.Property(cm => cm.UpdatedAt).IsRequired();

        builder.HasOne<ChatChannel>()
            .WithMany()
            .HasForeignKey(cm => cm.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany(u => u.ChatMessages)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cm => cm.ChannelId);
        builder.HasIndex(cm => cm.UserId);
        builder.HasIndex(cm => cm.CreatedAt);
    }
}
