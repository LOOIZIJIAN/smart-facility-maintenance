using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Data;

namespace SmartFacilityMaintenance.Controllers;

// Ching Linn Kee - Maintenance Request Management
[Authorize]
public class RequestController : Controller
{
    private readonly ApplicationDbContext _db;

    public RequestController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _db.MaintenanceRequests
            .Include(r => r.Category)
            .Include(r => r.Building)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(requests);
    }
}
