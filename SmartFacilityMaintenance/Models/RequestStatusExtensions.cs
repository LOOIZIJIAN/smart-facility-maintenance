namespace SmartFacilityMaintenance.Models;

public static class RequestStatusExtensions
{
    public static string ToDisplayName(this RequestStatus status) => status switch
    {
        RequestStatus.Submitted => "Submitted",
        RequestStatus.Assigned => "Assigned",
        RequestStatus.InProgress => "In Progress",
        RequestStatus.OnHold => "On Hold",
        RequestStatus.Completed => "Completed",
        RequestStatus.Closed => "Closed",
        RequestStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    public static string ToBadgeClass(this RequestStatus status) => status switch
    {
        RequestStatus.Submitted => "bg-secondary",
        RequestStatus.Assigned => "bg-info text-dark",
        RequestStatus.InProgress => "bg-warning text-dark",
        RequestStatus.OnHold => "bg-secondary",
        RequestStatus.Completed => "bg-success",
        RequestStatus.Closed => "bg-primary",
        RequestStatus.Cancelled => "bg-dark",
        _ => "bg-secondary"
    };
}
