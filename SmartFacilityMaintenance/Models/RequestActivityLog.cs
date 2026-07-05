using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models;

public class RequestActivityLog
{
    public int Id { get; set; }

    public int MaintenanceRequestId { get; set; }
    public MaintenanceRequest? MaintenanceRequest { get; set; }

    [Required]
    public string PerformedById { get; set; } = string.Empty;
    public ApplicationUser? PerformedBy { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    public RequestStatus? OldStatus { get; set; }
    public RequestStatus? NewStatus { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
