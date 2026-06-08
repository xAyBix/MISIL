using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Misil.Api.Hubs;
using Misil.Api.Middleware;
using Misil.Api.Services;
using Misil.Application;
using Misil.Infrastructure;
using Misil.Infrastructure.Data;
using Misil.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Misil.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddOpenApi();

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ProjectHub>("/hubs/project");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MisilDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await context.Database.EnsureCreatedAsync();

    await context.Database.ExecuteSqlRawAsync(@"
        ALTER TABLE ""AuditLogs"" ADD COLUMN IF NOT EXISTS ""UserName"" text NOT NULL DEFAULT '';
    ");

    await context.Database.ExecuteSqlRawAsync(@"
        ALTER TABLE ""Issues"" ADD COLUMN IF NOT EXISTS ""AssigneeTeamId"" uuid NULL;
    ");

    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ""ProjectInvitations"" (
            ""Id"" uuid NOT NULL,
            ""ProjectId"" uuid NOT NULL,
            ""InvitedUserId"" uuid NOT NULL,
            ""InvitedByUserId"" uuid NOT NULL,
            ""RoleId"" uuid NOT NULL,
            ""Status"" character varying(20) NOT NULL,
            ""CreatedAt"" timestamp with time zone NOT NULL,
            ""RespondedAt"" timestamp with time zone,
            CONSTRAINT ""PK_ProjectInvitations"" PRIMARY KEY (""Id""),
            CONSTRAINT ""FK_ProjectInvitations_Projects_ProjectId"" FOREIGN KEY (""ProjectId"") REFERENCES ""Projects""(""Id"") ON DELETE CASCADE,
            CONSTRAINT ""FK_ProjectInvitations_Users_InvitedByUserId"" FOREIGN KEY (""InvitedByUserId"") REFERENCES ""Users""(""Id""),
            CONSTRAINT ""FK_ProjectInvitations_Users_InvitedUserId"" FOREIGN KEY (""InvitedUserId"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS ""IX_ProjectInvitations_InvitedByUserId"" ON ""ProjectInvitations"" (""InvitedByUserId"");
        CREATE INDEX IF NOT EXISTS ""IX_ProjectInvitations_InvitedUserId"" ON ""ProjectInvitations"" (""InvitedUserId"");
        CREATE INDEX IF NOT EXISTS ""IX_ProjectInvitations_ProjectId"" ON ""ProjectInvitations"" (""ProjectId"");
    ");

    await SeedData.SeedAsync(context, userManager, roleManager);
}

app.Run();
