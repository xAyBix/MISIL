using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;

namespace Misil.Infrastructure.Data.Seed;
public static class SeedData
{
    public static async Task SeedAsync(MisilDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        if (!await context.ProjectRoles.AnyAsync())
        {
            context.ProjectRoles.Add(new Misil.Domain.Entities.Role
            {
                Id = Guid.NewGuid(), Name = "Owner", Description = "Full project control",
                Permissions = "all"
            });
            context.ProjectRoles.Add(new Misil.Domain.Entities.Role
            {
                Id = Guid.NewGuid(), Name = "Admin", Description = "Manage members and roles",
                Permissions = "invite_members,remove_members,manage_roles,edit_project,view_stats,manage_teams,manage_channels,create_issues,manage_sprints"
            });
            context.ProjectRoles.Add(new Misil.Domain.Entities.Role
            {
                Id = Guid.NewGuid(), Name = "Member", Description = "Regular project member",
                Permissions = "view_stats"
            });
            context.ProjectRoles.Add(new Misil.Domain.Entities.Role
            {
                Id = Guid.NewGuid(), Name = "Viewer", Description = "Read-only access",
                Permissions = ""
            });
            await context.SaveChangesAsync();
        }

        var roles = new[] { "Admin", "ProjectManager", "Developer", "Viewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        var adminUser = await userManager.FindByEmailAsync("admin@misil.dev");
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "admin",
                Email = "admin@misil.dev",
                AvatarUrl = null,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var demoUser = await userManager.FindByEmailAsync("user@misil.dev");
        if (demoUser == null)
        {
            demoUser = new User
            {
                UserName = "user",
                Email = "user@misil.dev",
                AvatarUrl = null,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(demoUser, "User123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(demoUser, "Developer");
        }
    }
}
