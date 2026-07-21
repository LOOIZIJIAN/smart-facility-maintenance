using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Notifications
    // Shows only notifications belonging to the logged-in user.
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();

        ViewBag.UnreadCount =
            notifications.Count(n => !n.IsRead);

        return View(notifications);
    }

    // POST: /Notifications/MarkAsRead
    // Any logged-in user can mark only their own notification as read.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.NotificationId == id &&
                n.UserId == userId);

        if (notification is null)
        {
            return NotFound();
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] =
            "Notification marked as read.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Notifications/MarkAllAsRead
    // Any logged-in user can mark all their own notifications as read.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var notifications = await _context.Notifications
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        if (notifications.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] =
            notifications.Count > 0
                ? "All notifications were marked as read."
                : "There are no unread notifications.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Notifications/Delete
    // Number 2 fix:
    // Only Admin can delete notifications.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id);

        if (notification is null)
        {
            return NotFound();
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Notification deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Notifications/CreateTestNotification
    // Number 3 fix:
    // Only Admin can create a test notification.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateTestNotification()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var notification = new Notification
        {
            UserId = userId,
            Title = "Test Notification",
            Message =
                "This is a test notification created by the administrator.",
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Test notification created successfully.";

        return RedirectToAction(nameof(Index));
    }
}