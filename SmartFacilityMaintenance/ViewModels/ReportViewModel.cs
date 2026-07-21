namespace SmartFacilityMaintenance.ViewModels;

public class ReportViewModel
{
    // Summary cards
    public int TotalRequests { get; set; }
    public int SubmittedRequests { get; set; }
    public int AssignedRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int OnHoldRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int CancelledRequests { get; set; }

    public double AverageCompletionHours { get; set; }

    // Requests by status
    public List<string> StatusLabels { get; set; } = new();
    public List<int> StatusCounts { get; set; } = new();

    // Requests by building
    public List<string> BuildingLabels { get; set; } = new();
    public List<int> BuildingCounts { get; set; } = new();

    // Requests by department
    public List<string> DepartmentLabels { get; set; } = new();
    public List<int> DepartmentCounts { get; set; } = new();

    // Monthly requests
    public List<string> MonthlyLabels { get; set; } = new();
    public List<int> MonthlyCounts { get; set; } = new();

    // Most reported categories
    public List<string> CategoryLabels { get; set; } = new();
    public List<int> CategoryCounts { get; set; } = new();
}