using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly IApiClient _api;

    public EventsController(IApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result =
            await _api.GetAsync<List<EventViewModel>>("api/events");

        return View(result.Data ?? []);
    }
}