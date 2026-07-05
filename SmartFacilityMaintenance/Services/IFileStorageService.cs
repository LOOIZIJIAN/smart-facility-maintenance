using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Services;

public interface IFileStorageService
{
    Task<RequestAttachment> SaveAsync(IFormFile file);
    void Delete(RequestAttachment attachment);
}
