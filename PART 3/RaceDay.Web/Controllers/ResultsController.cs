using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Participant")]
public class ResultsController : Controller
{
    private readonly IApiClient _api;

    public ResultsController(IApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result =
            await _api.GetAsync<List<ResultViewModel>>
            ("api/users/me/results");

        return View(result.Data ?? []);
    }
}