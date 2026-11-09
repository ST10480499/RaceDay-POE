namespace RaceDay.Api.Services;

public record StoredFile(Stream Content, string ContentType);

public interface IFileStorage
{
    Task SaveAsync(string container, string baseName, string extension, Stream content);
    Task<StoredFile?> OpenAsync(string container, string baseName);
}

// Saves files in an Uploads folder. This class is the only place that knows
// where files live, so it can be replaced by an Azure Blob version later.
public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "Uploads");
    }

    public async Task SaveAsync(string container, string baseName, string extension, Stream content)
    {
        var folder = Path.Combine(_root, container);
        Directory.CreateDirectory(folder);

        foreach (var old in Directory.GetFiles(folder, baseName + ".*"))
        {
            File.Delete(old);
        }

        var path = Path.Combine(folder, baseName + extension);
        using var output = File.Create(path);
        await content.CopyToAsync(output);
    }

    public Task<StoredFile?> OpenAsync(string container, string baseName)
    {
        var folder = Path.Combine(_root, container);
        if (!Directory.Exists(folder)) return Task.FromResult<StoredFile?>(null);

        var found = Directory.GetFiles(folder, baseName + ".*").FirstOrDefault();
        if (found == null) return Task.FromResult<StoredFile?>(null);

        var type = Path.GetExtension(found).ToLower() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        return Task.FromResult<StoredFile?>(new StoredFile(File.OpenRead(found), type));
    }
}
