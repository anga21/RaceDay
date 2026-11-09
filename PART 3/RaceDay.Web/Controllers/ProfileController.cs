using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IApiClient _api;

    public ProfileController(IApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result =
            await _api.GetAsync<ProfileViewModel>("api/users/me");

        return View(result.Data);
    }
}