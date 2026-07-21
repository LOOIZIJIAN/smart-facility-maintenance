using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;
using SmartFacilityMaintenance.Models.ViewModels;
using SmartFacilityMaintenance.Services;
using SmartFacilityMaintenance.Services.Reports;

namespace SmartFacilityMaintenance.Controllers;

// Ching Linn Kee - Maintenance Request Management
// Harenthran - In-app notification integration
[Authorize]
public class RequestController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public RequestController(
        ApplicationDbContext db,
        IFileStorageService fileStorage,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    // GET: /Request
    // GET: /Request/Index?id=5
    public async Task<IActionResult> Index(int? id)
    {
        if (User.IsInRole(Roles.Student))
        {
            await LoadLookupsAsync();

            return View(
                "StudentSubmit",
                new MaintenanceRequestCreateViewModel());
        }

        return await BuildBrowseView(id);
    }

    // POST: /Request/Submit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Submit(
        MaintenanceRequestCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return View("StudentSubmit", vm);
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

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
            foreach (var file in vm.Attachments
                         .Where(f => f is { Length: > 0 })
                         .Take(5))
            {
                try
                {
                    var attachment =
                        await _fileStorage.SaveAsync(file);

                    attachment.MaintenanceRequestId = request.Id;

                    _db.RequestAttachments.Add(attachment);
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
        }

        _db.RequestActivityLogs.Add(
            new RequestActivityLog
            {
                MaintenanceRequestId = request.Id,
                PerformedById = userId,
                Action = "Created",
                NewStatus = RequestStatus.Submitted,
                Notes = "Request submitted by student.",
                Timestamp = now
            });

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(
            userId,
            "Request Submitted",
            $"Your maintenance request #{request.Id} " +
            $"\"{request.Title}\" was submitted successfully.",
            request.Id);

        await NotifyOperationalUsersAboutNewRequestAsync(request);

        TempData["Success"] =
            $"Request #{request.Id} \"{request.Title}\" " +
            "has been submitted.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Request/StartMission/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Maintenance)]
    public Task<IActionResult> StartMission(int id)
    {
        return ChangeStatus(
            id: id,
            newStatus: RequestStatus.InProgress,
            note: "Maintenance started working on this request.",
            notificationTitle: "Request In Progress",
            notificationMessage:
                "Maintenance has started working on your request.",
            assignToCurrentUser: true);
    }

    // POST: /Request/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Maintenance)]
    public Task<IActionResult> Complete(int id)
    {
        return ChangeStatus(
            id: id,
            newStatus: RequestStatus.Completed,
            note: "Maintenance marked this request as completed.",
            notificationTitle: "Request Completed",
            notificationMessage:
                "Your maintenance request has been completed.",
            assignToCurrentUser: true,
            setCompletedAt: true);
    }

    // POST: /Request/Close/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Staff},{Roles.Admin}")]
    public async Task<IActionResult> Close(int id)
    {
        var request = await _db.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (request.Status == RequestStatus.Closed)
        {
            TempData["Error"] =
                $"Request #{request.Id} has already been closed.";

            return RedirectToAction(
                nameof(Index),
                new { id = request.Id });
        }

        if (request.Status != RequestStatus.Completed)
        {
            TempData["Error"] =
                $"Request #{request.Id} must be completed before it can be closed.";

            return RedirectToAction(
                nameof(Index),
                new { id = request.Id });
        }

        var now = DateTime.UtcNow;

        request.Status = RequestStatus.Closed;
        request.UpdatedAt = now;

        if (request.CompletedAt is null)
        {
            request.CompletedAt = now;
        }

        _db.RequestActivityLogs.Add(
            new RequestActivityLog
            {
                MaintenanceRequestId = request.Id,
                PerformedById = userId,
                Action = "Closed",
                OldStatus = RequestStatus.Completed,
                NewStatus = RequestStatus.Closed,
                Notes = "Request closed by staff/administrator.",
                Timestamp = now
            });

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(
            request.ReportedById,
            "Request Closed",
            $"Your maintenance request #{request.Id} " +
            $"\"{request.Title}\" has been reviewed and closed.",
            request.Id);

        TempData["Success"] =
            $"Request #{request.Id} has been closed successfully.";

        return RedirectToAction(
            nameof(Index),
            new { id = request.Id });
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Buildings = await _db.Buildings
            .OrderBy(b => b.Name)
            .ToListAsync();
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

        var currentId =
            id.HasValue && ids.Contains(id.Value)
                ? id.Value
                : ids[0];

        var request = await _db.MaintenanceRequests
            .Include(r => r.Category)
            .Include(r => r.Building)
            .Include(r => r.ReportedBy)
            .Include(r => r.AssignedTo)
            .Include(r => r.Attachments)
            .FirstAsync(r => r.Id == currentId);

        var index = ids.IndexOf(currentId);

        ViewBag.PreviousId =
            index > 0
                ? ids[index - 1]
                : (int?)null;

        ViewBag.NextId =
            index < ids.Count - 1
                ? ids[index + 1]
                : (int?)null;

        ViewBag.Position = index + 1;
        ViewBag.Total = ids.Count;

        return View("Browse", request);
    }

    private async Task<IActionResult> ChangeStatus(
        int id,
        RequestStatus newStatus,
        string note,
        string notificationTitle,
        string notificationMessage,
        bool assignToCurrentUser = false,
        bool setCompletedAt = false)
    {
        var request = await _db.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request is null)
        {
            return NotFound();
        }

        if (request.Status == newStatus)
        {
            TempData["Error"] =
                $"Request #{request.Id} is already marked as " +
                $"\"{newStatus.ToDisplayName()}\".";

            return RedirectToAction(
                nameof(Index),
                new { id = request.Id });
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var oldStatus = request.Status;
        var now = DateTime.UtcNow;

        request.Status = newStatus;
        request.UpdatedAt = now;

        if (setCompletedAt)
        {
            request.CompletedAt = now;
        }

        if (assignToCurrentUser &&
            string.IsNullOrWhiteSpace(request.AssignedToId))
        {
            request.AssignedToId = userId;
        }

        _db.RequestActivityLogs.Add(
            new RequestActivityLog
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

        await _notificationService.CreateAsync(
            request.ReportedById,
            notificationTitle,
            $"{notificationMessage} Request #{request.Id}: " +
            $"\"{request.Title}\".",
            request.Id);

        TempData["Success"] =
            $"Request #{request.Id} updated to " +
            $"\"{newStatus.ToDisplayName()}\".";

        return RedirectToAction(
            nameof(Index),
            new { id = request.Id });
    }

    private async Task NotifyOperationalUsersAboutNewRequestAsync(
        MaintenanceRequest request)
    {
        var notifiedUserIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var adminUsers =
            await _userManager.GetUsersInRoleAsync(Roles.Admin);

        var staffUsers =
            await _userManager.GetUsersInRoleAsync(Roles.Staff);

        var maintenanceUsers =
            await _userManager.GetUsersInRoleAsync(Roles.Maintenance);

        var recipients = adminUsers
            .Concat(staffUsers)
            .Concat(maintenanceUsers);

        foreach (var recipient in recipients)
        {
            if (string.IsNullOrWhiteSpace(recipient.Id) ||
                !notifiedUserIds.Add(recipient.Id))
            {
                continue;
            }

            await _notificationService.CreateAsync(
                recipient.Id,
                "New Maintenance Request",
                $"A new maintenance request #{request.Id} " +
                $"\"{request.Title}\" has been submitted and requires attention.",
                request.Id);
        }
    }
}