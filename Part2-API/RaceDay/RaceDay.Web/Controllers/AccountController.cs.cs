using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.LoginAsync(model.Email, model.Password);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login failed.");
            return View(model);
        }

        HttpContext.Session.SetString("UserRole", result.Role ?? "Participant");
        HttpContext.Session.SetString("UserName", result.Name ?? model.Email);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.RegisterAsync(model.FullName, model.Email, model.Password, model.Role);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed.");
            return View(model);
        }

        TempData["Success"] = "Account created! Please log in.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.LogoutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}