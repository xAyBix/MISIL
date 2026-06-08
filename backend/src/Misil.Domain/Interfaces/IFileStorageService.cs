namespace Misil.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(string entityType, string entityId, string fileName, Stream content, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
    string GetUrl(string relativePath);
}
