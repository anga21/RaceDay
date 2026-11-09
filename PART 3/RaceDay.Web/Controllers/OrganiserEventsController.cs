using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserEventsController : Controller
{
    public IActionResult Index()
    {
        return View(new List<OrganiserEventViewModel>());
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult Edit(int id)
    {
        var model = new EventViewModel();

        return View(model);
    }

    public IActionResult Delete(int id)
    {
        return View();
    }
}