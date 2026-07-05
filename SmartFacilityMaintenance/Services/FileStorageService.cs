using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Services;

public class FileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx"
    };

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    private string UploadsPath => Path.Combine(_environment.WebRootPath, "uploads");

    public async Task<RequestAttachment> SaveAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("No file was provided.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File exceeds the 5 MB limit.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("File type is not allowed.");

        Directory.CreateDirectory(UploadsPath);

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(UploadsPath, storedName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return new RequestAttachment
        {
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedName,
            ContentType = file.ContentType,
            FileSize = file.Length
        };
    }

    public void Delete(RequestAttachment attachment)
    {
        if (attachment is null || string.IsNullOrEmpty(attachment.StoredFileName))
            return;

        var fullPath = Path.Combine(UploadsPath, attachment.StoredFileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
