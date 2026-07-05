using System.ComponentModel.DataAnnotations;

namespace SmartFacilityMaintenance.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    public ICollection<MaintenanceRequest> Requests { get; set; } = new List<MaintenanceRequest>();
}
