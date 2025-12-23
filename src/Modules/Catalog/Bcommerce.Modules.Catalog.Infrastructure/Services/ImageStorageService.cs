namespace Bcommerce.Modules.Catalog.Infrastructure.Services;

public class ImageStorageService
{
    // Placeholder implementation as requested by user based on file list provided
    // Ideally this should implement an interface from Application layer
    
    public Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        // TODO: Implement actual storage logic (e.g. Azure Blob Storage, AWS S3, Local Disk)
        return Task.FromResult($"https://cdn.bcommerce.com/images/{Guid.NewGuid()}-{fileName}");
    }
}
