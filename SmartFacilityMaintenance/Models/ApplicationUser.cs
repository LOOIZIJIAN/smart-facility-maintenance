using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SmartFacilityMaintenance.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(20)]
    [Display(Name = "Staff / Student ID")]
    public string? StaffStudentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MaintenanceRequest> ReportedRequests { get; set; } = new List<MaintenanceRequest>();
    public ICollection<MaintenanceRequest> AssignedRequests { get; set; } = new List<MaintenanceRequest>();
}
