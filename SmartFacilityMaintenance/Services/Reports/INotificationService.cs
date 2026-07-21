namespace SmartFacilityMaintenance.Services.Reports;

public interface INotificationService
{
    Task CreateAsync(
        string userId,
        string title,
        string message,
        int? requestId = null);
}