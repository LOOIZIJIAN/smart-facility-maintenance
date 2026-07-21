using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFacilityMaintenance.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }


        // User who receives the notification
        [Required]
        public string UserId { get; set; } = string.Empty;


        // Related maintenance request (optional)
        public int? RequestId { get; set; }


        // Notification title
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;


        // Notification message
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;


        // Notification status
        public bool IsRead { get; set; } = false;


        // Created time
        public DateTime CreatedDate { get; set; } = DateTime.Now;



        // Navigation to Maintenance Request
        [ForeignKey("RequestId")]
        public MaintenanceRequest? MaintenanceRequest { get; set; }


        // Navigation to User
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}