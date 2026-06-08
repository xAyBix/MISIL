namespace Misil.Api.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid projectId, string permission);
    Task<bool> IsOwnerAsync(Guid projectId);
}
