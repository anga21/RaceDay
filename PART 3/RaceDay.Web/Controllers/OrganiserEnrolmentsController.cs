using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserEnrolmentsController : Controller
{
    public IActionResult Index()
    {
        return View(new List<OrganiserEnrolmentViewModel>());
    }
}