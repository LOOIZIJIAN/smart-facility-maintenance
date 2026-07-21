using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Controllers;

// Grace Go Ying Chee - Request Tracking & History
[Authorize]
public class TrackingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TrackingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(
    string? search,
    int? categoryId,
    int? buildingId,
    RequestStatus? status,
    DateTime? dateFrom,
    DateTime? dateTo)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Challenge();
        }

        var query = _context.MaintenanceRequests
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Building)
            .Include(r => r.AssignedTo)
            .AsQueryable();

        // Students may only view their own submitted requests.
        if (User.IsInRole(Roles.Student))
        {
            query = query.Where(r => r.ReportedById == currentUser.Id);
        }

        // Search by title, description, building or room/location.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();

            query = query.Where(r =>
                r.Title.Contains(keyword) ||
                r.Description.Contains(keyword) ||
                r.RoomLocation.Contains(keyword) ||
                r.Building!.Name.Contains(keyword));
        }

        // Filter by category.
        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        // Filter by building.
        if (buildingId.HasValue)
        {
            query = query.Where(r => r.BuildingId == buildingId.Value);
        }

        // Filter by request status.
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        // Filter from the beginning of the selected date.
        if (dateFrom.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= dateFrom.Value.Date);
        }

        // Include the entire selected ending date.
        if (dateTo.HasValue)
        {
            var endDate = dateTo.Value.Date.AddDays(1);

            query = query.Where(r => r.CreatedAt < endDate);
        }

        // Preserve filter values when the page reloads.
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.BuildingId = buildingId;
        ViewBag.Status = status;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

        // Data for category and building dropdowns.
        ViewBag.Categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Buildings = await _context.Buildings
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();

        var requests = await query
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();

        return View(requests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Challenge();
        }

        var request = await _context.MaintenanceRequests
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Building)
            .Include(r => r.ReportedBy)
            .Include(r => r.AssignedTo)
            .Include(r => r.Attachments)
            .Include(r => r.ActivityLogs)
                .ThenInclude(log => log.PerformedBy)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        // Students can only open their own requests.
        if (User.IsInRole(Roles.Student) &&
            request.ReportedById != currentUser.Id)
        {
            return Forbid();
        }

        // Display the oldest activity first to form a timeline.
        request.ActivityLogs = request.ActivityLogs
            .OrderBy(log => log.Timestamp)
            .ToList();

        return View(request);
    }
}