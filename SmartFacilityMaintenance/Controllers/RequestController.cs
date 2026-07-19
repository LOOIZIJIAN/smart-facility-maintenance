using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;
using SmartFacilityMaintenance.Models.ViewModels;
using SmartFacilityMaintenance.Services;

namespace SmartFacilityMaintenance.Controllers;

// Ching Linn Kee - Maintenance Request Management
// - Student role  : submit a new request (with attachments)
// - Maintenance   : browse every request one at a time, Start Mission / Completed
// - Staff / Admin : browse every request one at a time, Close Maintenance Request
[Authorize]
public class RequestController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly UserManager<ApplicationUser> _userManager;

    public RequestController(ApplicationDbContext db, IFileStorageService fileStorage, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _fileStorage = fileStorage;
        _userManager = userManager;
    }

    // GET /Request  (and /Request/Index?id=5 for prev/next navigation)
    public async Task<IActionResult> Index(int? id)
    {
        if (User.IsInRole(Roles.Student))
        {
            await LoadLookupsAsync();
            return View("StudentSubmit", new MaintenanceRequestCreateViewModel());
        }

        return await BuildBrowseView(id);
    }

    // POST /Request/Submit — Student submits a new maintenance request
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Submit(MaintenanceRequestCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return View("StudentSubmit", vm);
        }

        var userId = _userManager.GetUserId(User)!;
        var now = DateTime.UtcNow;

        var request = new MaintenanceRequest
        {
            Title = vm.Title,
            Description = vm.Description,
            CategoryId = vm.CategoryId,
            BuildingId = vm.BuildingId,
            RoomLocation = vm.RoomLocation,
            Priority = vm.Priority,
            Status = RequestStatus.Submitted,
            ReportedById = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.MaintenanceRequests.Add(request);
        await _db.SaveChangesAsync();

        if (vm.Attachments is not null)
        {
            foreach (var file in vm.Attachments.Where(f => f is { Length: > 0 }).Take(5))
            {
                try
                {
                    var attachment = await _fileStorage.SaveAsync(file);
                    attachment.MaintenanceRequestId = request.Id;
                    _db.RequestAttachments.Add(attachment);
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
        }

        _db.RequestActivityLogs.Add(new RequestActivityLog
        {
            MaintenanceRequestId = request.Id,
            PerformedById = userId,
            Action = "Created",
            NewStatus = RequestStatus.Submitted,
            Notes = "Request submitted by student.",
            Timestamp = now
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Request #{request.Id} \"{request.Title}\" has been submitted.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Request/StartMission/5 — Maintenance staff begins work
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Maintenance)]
    public Task<IActionResult> StartMission(int id) =>
        ChangeStatus(id, RequestStatus.InProgress, "Maintenance started working on this request.", assignToCurrentUser: true);

    // POST /Request/Complete/5 — Maintenance staff finishes the physical work
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Maintenance)]
    public Task<IActionResult> Complete(int id) =>
        ChangeStatus(id, RequestStatus.Completed, "Maintenance marked this request as done.", assignToCurrentUser: true, setCompletedAt: true);

    // POST /Request/Close/5 — Staff / Admin closes the request
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Staff},{Roles.Admin}")]
    public Task<IActionResult> Close(int id) =>
        ChangeStatus(id, RequestStatus.Completed, "Request closed by staff/administrator.", setCompletedAt: true);

    private async Task LoadLookupsAsync()
    {
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Buildings = await _db.Buildings.OrderBy(b => b.Name).ToListAsync();
    }

    private async Task<IActionResult> BuildBrowseView(int? id)
    {
        var ids = await _db.MaintenanceRequests
            .OrderBy(r => r.CreatedAt)
            .Select(r => r.Id)
            .ToListAsync();

        if (ids.Count == 0)
        {
            ViewBag.PreviousId = null;
            ViewBag.NextId = null;
            ViewBag.Position = 0;
            ViewBag.Total = 0;
            return View("Browse", null);
        }

        var currentId = id.HasValue && ids.Contains(id.Value) ? id.Value : ids[0];

        var request = await _db.MaintenanceRequests
            .Include(r => r.Category)
            .Include(r => r.Building)
            .Include(r => r.ReportedBy)
            .Include(r => r.AssignedTo)
            .Include(r => r.Attachments)
            .FirstAsync(r => r.Id == currentId);

        var index = ids.IndexOf(currentId);
        ViewBag.PreviousId = index > 0 ? ids[index - 1] : (int?)null;
        ViewBag.NextId = index < ids.Count - 1 ? ids[index + 1] : (int?)null;
        ViewBag.Position = index + 1;
        ViewBag.Total = ids.Count;

        return View("Browse", request);
    }

    private async Task<IActionResult> ChangeStatus(
        int id, RequestStatus newStatus, string note,
        bool assignToCurrentUser = false, bool setCompletedAt = false)
    {
        var request = await _db.MaintenanceRequests.FindAsync(id);
        if (request is null)
        {
            return NotFound();
        }

        var oldStatus = request.Status;
        var userId = _userManager.GetUserId(User)!;
        var now = DateTime.UtcNow;

        request.Status = newStatus;
        request.UpdatedAt = now;
        if (setCompletedAt) request.CompletedAt = now;
        if (assignToCurrentUser && string.IsNullOrEmpty(request.AssignedToId)) request.AssignedToId = userId;

        _db.RequestActivityLogs.Add(new RequestActivityLog
        {
            MaintenanceRequestId = request.Id,
            PerformedById = userId,
            Action = "StatusChanged",
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Notes = note,
            Timestamp = now
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Request #{request.Id} updated to \"{newStatus.ToDisplayName()}\".";
        return RedirectToAction(nameof(Index), new { id = request.Id });
    }
}
