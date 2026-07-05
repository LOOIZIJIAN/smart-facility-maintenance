using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(20)]
    [Display(Name = "Staff / Student ID")]
    public string? StaffStudentId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
