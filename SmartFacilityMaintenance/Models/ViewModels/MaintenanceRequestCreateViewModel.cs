using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models.ViewModels;

public class MaintenanceRequestCreateViewModel
{
    [Required(ErrorMessage = "Please give the request a short title.")]
    [MaxLength(150)]
    [Display(Name = "Issue Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please describe the issue.")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Please select a building.")]
    [Display(Name = "Building")]
    public int BuildingId { get; set; }

    [Required(ErrorMessage = "Please state the room / location.")]
    [MaxLength(100)]
    [Display(Name = "Room / Location")]
    public string RoomLocation { get; set; } = string.Empty;

    [Display(Name = "Priority")]
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;

    [Display(Name = "Supporting Images / Documents")]
    public List<IFormFile>? Attachments { get; set; }
}
