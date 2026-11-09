using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController : Controller
{
    public IActionResult Dashboard()
    {
        var model = new DashboardViewModel
        {
            TotalEvents = 0,
            TotalEnrolments = 0,
            UpcomingEvents = 0
        };

        return View(model);
    }
}