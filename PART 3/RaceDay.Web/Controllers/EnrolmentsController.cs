using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Participant")]
public class EnrolmentsController : Controller
{
    private readonly IApiClient _api;

    public EnrolmentsController(IApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result =
        await _api.GetAsync<List<EnrolmentViewModel>>
        ("api/users/me/enrolments");

        return View(result.Data ?? []);
    }
}