using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[RequireRole]
public class ProfileController : Controller
{
    private readonly ApiService _api;

    public ProfileController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<ProfileFormModel>("api/users/me");

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!result.Success || result.Data == null)
        {
            ViewBag.Error = result.Error;
            return View(new ProfileFormModel());
        }

        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PutAsync<ProfileFormModel>("api/users/me",
            new { FullName = model.FullName, Email = model.Email });

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Your profile could not be updated.");
            return View(model);
        }

        HttpContext.Session.SetString("UserName", model.FullName);
        TempData["Success"] = "Profile updated.";
        return RedirectToAction("Index");
    }
}
