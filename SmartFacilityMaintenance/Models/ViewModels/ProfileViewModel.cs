using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models.ViewModels;

public class ProfileViewModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(20)]
    [Display(Name = "Staff / Student ID")]
    public string? StaffStudentId { get; set; }
}
