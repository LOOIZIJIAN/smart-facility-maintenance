using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;
using SmartFacilityMaintenance.ViewModels;

namespace SmartFacilityMaintenance.Controllers;

// Harenthran Chandrasegar - Reports & Analytics
[Authorize(Roles = "Admin,Staff,Maintenance")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _context.MaintenanceRequests
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Building)
            .Include(r => r.ReportedBy)
            .ToListAsync();

        var model = new ReportViewModel
        {
            TotalRequests = requests.Count,

            SubmittedRequests = requests.Count(
                r => r.Status == RequestStatus.Submitted),

            AssignedRequests = requests.Count(
                r => r.Status == RequestStatus.Assigned),

            InProgressRequests = requests.Count(
                r => r.Status == RequestStatus.InProgress),

            OnHoldRequests = requests.Count(
                r => r.Status == RequestStatus.OnHold),

            CompletedRequests = requests.Count(
                r => r.Status == RequestStatus.Completed),

            CancelledRequests = requests.Count(
                r => r.Status == RequestStatus.Cancelled)
        };

        BuildStatusReport(model, requests);
        BuildBuildingReport(model, requests);
        BuildDepartmentReport(model, requests);
        BuildMonthlyReport(model, requests);
        BuildCategoryReport(model, requests);
        BuildAverageCompletionTime(model, requests);

        return View(model);
    }

    private static void BuildStatusReport(
        ReportViewModel model,
        List<MaintenanceRequest> requests)
    {
        var statuses = Enum.GetValues<RequestStatus>();

        foreach (var status in statuses)
        {
            model.StatusLabels.Add(FormatStatus(status));
            model.StatusCounts.Add(
                requests.Count(r => r.Status == status));
        }
    }

    private static void BuildBuildingReport(
        ReportViewModel model,
        List<MaintenanceRequest> requests)
    {
        var buildingData = requests
            .GroupBy(r => r.Building?.Name ?? "Unknown Building")
            .Select(group => new
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Name)
            .ToList();

        model.BuildingLabels = buildingData
            .Select(item => item.Name)
            .ToList();

        model.BuildingCounts = buildingData
            .Select(item => item.Count)
            .ToList();
    }

    private static void BuildDepartmentReport(
        ReportViewModel model,
        List<MaintenanceRequest> requests)
    {
        var departmentData = requests
            .GroupBy(r =>
                string.IsNullOrWhiteSpace(r.ReportedBy?.Department)
                    ? "Not Specified"
                    : r.ReportedBy.Department)
            .Select(group => new
            {
                Name = group.Key!,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Name)
            .ToList();

        model.DepartmentLabels = departmentData
            .Select(item => item.Name)
            .ToList();

        model.DepartmentCounts = departmentData
            .Select(item => item.Count)
            .ToList();
    }

    private static void BuildMonthlyReport(
        ReportViewModel model,
        List<MaintenanceRequest> requests)
    {
        var currentMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1);

        // Display the latest six months.
        for (var monthsAgo = 5; monthsAgo >= 0; monthsAgo--)
        {
            var monthStart = currentMonth.AddMonths(-monthsAgo);
            var nextMonth = monthStart.AddMonths(1);

            model.MonthlyLabels.Add(
                monthStart.ToString("MMM yyyy"));

            model.MonthlyCounts.Add(
                requests.Count(r =>
                    r.CreatedAt >= monthStart &&
                    r.CreatedAt < nextMonth));
        }
    }

    private static void BuildCategoryReport(
        ReportViewModel model,
        List<MaintenanceRequest> requests)
    {
        var categoryData = requests
            .GroupBy(r => r.Category?.Name ?? "Uncategorised")
            .Select(group => new
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Name)
            .Take(5)
            .ToList();

        model.CategoryLabels = categoryData
            .Select(item => item.Name)
            .ToList();

        model.CategoryCounts = categoryData
            .Select(item => item.Count)
            .ToList();
    }

    private static void BuildAverageCompletionTime(
        ReportViewModel model,
        List<MaintenanceRequest> requests)
    {
        var completedDurations = requests
            .Where(r =>
                r.Status == RequestStatus.Completed &&
                r.CompletedAt.HasValue)
            .Select(r =>
                (r.CompletedAt!.Value - r.CreatedAt).TotalHours)
            .Where(hours => hours >= 0)
            .ToList();

        model.AverageCompletionHours =
            completedDurations.Count == 0
                ? 0
                : Math.Round(completedDurations.Average(), 1);
    }

    private static string FormatStatus(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.InProgress => "In Progress",
            RequestStatus.OnHold => "On Hold",
            _ => status.ToString()
        };
    }
}