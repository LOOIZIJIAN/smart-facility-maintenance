using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models;

public class RequestAttachment
{
    public int Id { get; set; }

    public int MaintenanceRequestId { get; set; }
    public MaintenanceRequest? MaintenanceRequest { get; set; }

    [Required]
    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(260)]
    public string StoredFileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
