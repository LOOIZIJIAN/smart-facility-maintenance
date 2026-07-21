using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Services.Reports;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        string userId,
        string title,
        string message,
        int? requestId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var notification = new Notification
        {
            UserId = userId,
            RequestId = requestId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}