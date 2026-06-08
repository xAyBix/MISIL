using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Misil.Domain.Entities;
using Misil.Domain.Enums;

namespace Misil.Infrastructure.Data.Configurations;
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.Property(al => al.UserName).IsRequired().HasMaxLength(256);
        builder.Property(al => al.EntityType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(al => al.Action).IsRequired().HasMaxLength(200);
        builder.Property(al => al.OldValue);
        builder.Property(al => al.NewValue);
        builder.Property(al => al.CreatedAt).IsRequired();

        builder.HasOne(al => al.Project)
            .WithMany(p => p.AuditLogs)
            .HasForeignKey(al => al.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(al => al.ProjectId);
        builder.HasIndex(al => al.EntityType);
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.CreatedAt);
    }
}
