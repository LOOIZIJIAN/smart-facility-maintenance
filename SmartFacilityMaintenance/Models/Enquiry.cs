using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models;

public class Enquiry
{
    public int Id { get; set; }

    public int MaintenanceRequestId { get; set; }
    public MaintenanceRequest? MaintenanceRequest { get; set; }

    [Required]
    public string RaisedById { get; set; } = string.Empty;
    public ApplicationUser? RaisedBy { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Response { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}
