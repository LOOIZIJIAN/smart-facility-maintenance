using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartFacilityMaintenance.Controllers;

// Nigel Ng Kai Shuen - Request Modification & Activity Logs
[Authorize]
public class ModificationController : Controller
{
    public IActionResult Index() => View();
}
