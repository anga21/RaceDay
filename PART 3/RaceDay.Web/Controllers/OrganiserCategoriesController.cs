using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserCategoriesController : Controller
{
    public IActionResult Index()
    {
        return View(new List<CategoryViewModel>());
    }

    public IActionResult Create()
    {
        return View();
    }
}