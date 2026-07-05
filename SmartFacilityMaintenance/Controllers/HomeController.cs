using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;
using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Dashboard));

        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Total = await _db.MaintenanceRequests.CountAsync();
        ViewBag.Open = await _db.MaintenanceRequests.CountAsync(r => r.Status != RequestStatus.Completed && r.Status != RequestStatus.Cancelled);
        ViewBag.Completed = await _db.MaintenanceRequests.CountAsync(r => r.Status == RequestStatus.Completed);
        ViewBag.HighPriority = await _db.MaintenanceRequests.CountAsync(r => r.Priority == RequestPriority.High && r.Status != RequestStatus.Completed && r.Status != RequestStatus.Cancelled);
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
