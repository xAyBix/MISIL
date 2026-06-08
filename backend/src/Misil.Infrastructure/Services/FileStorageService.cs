namespace Misil.Infrastructure.Services;
public interface IFileStorageService
{
    Task<string> SaveAsync(string entityType, string entityId, string fileName, Stream content);
    Task DeleteAsync(string filePath);
    string GetUrlAsync(string relativePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    }

    public async Task<string> SaveAsync(string entityType, string entityId, string fileName, Stream content)
    {
        var directory = Path.Combine(_basePath, entityType, entityId);
        Directory.CreateDirectory(directory);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(directory, uniqueFileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(stream);

        return Path.Combine(entityType, entityId, uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public string GetUrlAsync(string relativePath)
    {
        return $"/uploads/{relativePath}";
    }
}
