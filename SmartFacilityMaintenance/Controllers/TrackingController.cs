using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartFacilityMaintenance.Controllers;

// Grace Go Ying Chee - Request Tracking & History
[Authorize]
public class TrackingController : Controller
{
    public IActionResult Index() => View();
}
