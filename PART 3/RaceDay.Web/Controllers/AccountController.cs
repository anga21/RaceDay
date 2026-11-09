using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApiClient _api;

    public AccountController(IApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToRoleHome();
        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role == "Organiser" && string.IsNullOrWhiteSpace(model.OrganiserCode))
            ModelState.AddModelError(nameof(model.OrganiserCode),
                "An organiser code is required to register as an Organiser.");

        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<UserResponse>("api/auth/register", new
        {
            firstName = model.FirstName,
            lastName = model.LastName,
            phoneNumber = model.PhoneNumber,
            email = model.Email,
            password = model.Password,
            role = model.Role,
            organiserCode = model.Role == "Organiser" ? model.OrganiserCode : null
        });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "We could not create your account.");
            return View(model);
        }

        TempData["Success"] = "Your account has been created. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToRoleHome();
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (result, apiCookie) = await _api.LoginAsync<UserResponse>(new
        {
            email = model.Email,
            password = model.Password
        });

        if (!result.Success || result.Data == null || apiCookie == null)
        {
            var message = result.StatusCode == HttpStatusCode.Unauthorized
                ? "Incorrect email or password."
                : result.ErrorMessage ?? "We could not log you in.";
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        var user = result.Data;

        
        HttpContext.Session.SetString(ApiCookieHandler.SessionKey, apiCookie);

        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return user.Role == "Organiser"
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Events");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        
        await _api.PostNoContentAsync("api/auth/logout");

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToRoleHome() =>
    User.IsInRole("Organiser")
        ? RedirectToAction("Dashboard", "Organiser")
        : RedirectToAction("Index", "Events");

    [HttpGet]
    public IActionResult AccessDenied() => View();

    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ApiCheck()
    {
        var result = await _api.GetAsync<UserResponse>("api/users/me");
        return Content(result.Success
            ? $"The API recognises you as {result.Data!.FirstName} {result.Data.LastName} ({result.Data.Role})."
            : $"The API did not recognise you: {result.StatusCode}");
    }
}