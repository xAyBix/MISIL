using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Configurations;
public class ChatChannelConfiguration : IEntityTypeConfiguration<ChatChannel>
{
    public void Configure(EntityTypeBuilder<ChatChannel> builder)
    {
        builder.ToTable("ChatChannels");

        builder.Property(cc => cc.Name).IsRequired().HasMaxLength(200);
        builder.Property(cc => cc.CreatedAt).IsRequired();

        builder.HasOne(cc => cc.Project)
            .WithMany(p => p.Channels)
            .HasForeignKey(cc => cc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cc => cc.ProjectId);
    }
}
