namespace SmartFacilityMaintenance.Models;

public enum RequestStatus
{
    Submitted,
    Assigned,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}

public enum RequestPriority
{
    Low,
    Medium,
    High
}
