using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Controllers;

// Nigel Ng Kai Shuen - Request Modification & Activity Logs Module
[Authorize]
public class ModificationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ModificationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /Modification
    // Displays maintenance requests that can be reviewed for modification/cancellation.
    public async Task<IActionResult> Index()
    {
        var requests = await _db.MaintenanceRequests
            .Include(r => r.Category)
            .Include(r => r.Building)
            .Include(r => r.ReportedBy)
            .Include(r => r.AssignedTo)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();

        return View(requests);
    }

    // GET: /Modification/Details/5
    // Shows request details and its activity log.
    public async Task<IActionResult> Details(int id)
    {
        var request = await _db.MaintenanceRequests
            .Include(r => r.Category)
            .Include(r => r.Building)
            .Include(r => r.ReportedBy)
            .Include(r => r.AssignedTo)
            .Include(r => r.ActivityLogs)
                .ThenInclude(l => l.PerformedBy)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        return View(request);
    }

    // GET: /Modification/Edit/5
    // Allows users to edit request details before the request is processed.
    public async Task<IActionResult> Edit(int id)
    {
        var request = await _db.MaintenanceRequests
            .Include(r => r.Category)
            .Include(r => r.Building)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != RequestStatus.Submitted)
        {
            TempData["Error"] = "This request cannot be edited because it is already being processed.";
            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        return View(request);
    }

    // POST: /Modification/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MaintenanceRequest updatedRequest)
    {
        var request = await _db.MaintenanceRequests.FindAsync(id);

        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != RequestStatus.Submitted)
        {
            TempData["Error"] = "This request cannot be edited because it is already being processed.";
            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        var oldDetails =
            $"Title: {request.Title}; Description: {request.Description}; Room/Location: {request.RoomLocation}; Priority: {request.Priority}";

        request.Title = updatedRequest.Title;
        request.Description = updatedRequest.Description;
        request.RoomLocation = updatedRequest.RoomLocation;
        request.Priority = updatedRequest.Priority;
        request.UpdatedAt = DateTime.UtcNow;

        var newDetails =
            $"Title: {request.Title}; Description: {request.Description}; Room/Location: {request.RoomLocation}; Priority: {request.Priority}";

        var userId = _userManager.GetUserId(User) ?? string.Empty;

        _db.RequestActivityLogs.Add(new RequestActivityLog
        {
            MaintenanceRequestId = request.Id,
            PerformedById = userId,
            Action = "Request Edited",
            OldStatus = request.Status,
            NewStatus = request.Status,
            Notes = $"Request details updated. Old details: {oldDetails}. New details: {newDetails}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Request details were updated successfully.";
        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    // GET: /Modification/Cancel/5
    // Shows cancellation confirmation page.
    public async Task<IActionResult> Cancel(int id)
    {
        var request = await _db.MaintenanceRequests
            .Include(r => r.Category)
            .Include(r => r.Building)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        if (request.Status == RequestStatus.Completed || request.Status == RequestStatus.Cancelled)
        {
            TempData["Error"] = "This request cannot be cancelled because it is already completed or cancelled.";
            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        return View(request);
    }

    // POST: /Modification/CancelConfirmed/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(int id, string cancellationReason)
    {
        var request = await _db.MaintenanceRequests.FindAsync(id);

        if (request == null)
        {
            return NotFound();
        }

        if (request.Status == RequestStatus.Completed || request.Status == RequestStatus.Cancelled)
        {
            TempData["Error"] = "This request cannot be cancelled because it is already completed or cancelled.";
            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        var oldStatus = request.Status;

        request.Status = RequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        var userId = _userManager.GetUserId(User) ?? string.Empty;

        _db.RequestActivityLogs.Add(new RequestActivityLog
        {
            MaintenanceRequestId = request.Id,
            PerformedById = userId,
            Action = "Request Cancelled",
            OldStatus = oldStatus,
            NewStatus = RequestStatus.Cancelled,
            Notes = string.IsNullOrWhiteSpace(cancellationReason)
                ? "Request was cancelled by the user."
                : $"Request was cancelled. Reason: {cancellationReason}",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Request was cancelled successfully.";
        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    // GET: /Modification/ActivityLog/5
    // Displays all activity logs for a selected request.
    public async Task<IActionResult> ActivityLog(int id)
    {
        var request = await _db.MaintenanceRequests
            .Include(r => r.ActivityLogs)
                .ThenInclude(l => l.PerformedBy)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        return View(request);
    }
}