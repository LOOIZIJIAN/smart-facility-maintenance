using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartFacilityMaintenance.Controllers;

// Harenthran Chandrasegar - Reports & Analytics
[Authorize(Roles = "Admin,Staff,Maintenance")]
public class ReportsController : Controller
{
    public IActionResult Index() => View();
}
