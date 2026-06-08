using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Misil.Application.Behaviors;
using Misil.Application.Common.Services;
using System.Reflection;

namespace Misil.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
