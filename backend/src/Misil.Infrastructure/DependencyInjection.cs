using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Misil.Application.Common.Interfaces;
using Misil.Domain.Entities;
using Misil.Infrastructure.Data;
using Misil.Infrastructure.Repositories;
using Misil.Infrastructure.Services;
using System.Text;

namespace Misil.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MisilDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<MisilDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<Domain.Interfaces.IProjectRepository, ProjectRepository>();
        services.AddScoped<Domain.Interfaces.IIssueRepository, IssueRepository>();
        services.AddScoped<Domain.Interfaces.IChatRepository, ChatRepository>();
        services.AddScoped<Domain.Interfaces.IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<Domain.Interfaces.IUserRepository, UserRepository>();
        services.AddScoped<Domain.Interfaces.ITeamRepository, TeamRepository>();
        services.AddScoped<Domain.Interfaces.ISprintRepository, SprintRepository>();
        services.AddScoped<Domain.Interfaces.IGanttRepository, GanttRepository>();
        services.AddScoped<Domain.Interfaces.IRoleRepository, RoleRepository>();

        return services;
    }
}
