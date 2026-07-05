using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models;

public class MaintenanceRequest
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Display(Name = "Building")]
    public int BuildingId { get; set; }
    public Building? Building { get; set; }

    [MaxLength(100)]
    [Display(Name = "Room / Location")]
    public string RoomLocation { get; set; } = string.Empty;

    public RequestPriority Priority { get; set; } = RequestPriority.Medium;
    public RequestStatus Status { get; set; } = RequestStatus.Submitted;

    [Required]
    public string ReportedById { get; set; } = string.Empty;
    public ApplicationUser? ReportedBy { get; set; }

    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();
    public ICollection<RequestActivityLog> ActivityLogs { get; set; } = new List<RequestActivityLog>();
    public ICollection<Enquiry> Enquiries { get; set; } = new List<Enquiry>();
}
