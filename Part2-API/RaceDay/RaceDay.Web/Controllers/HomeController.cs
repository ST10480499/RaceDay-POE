using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiService _api;

    public HomeController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(string? search, string? type)
    {
        var role = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(role))
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Organiser");
        }

        ViewBag.Search = search;
        ViewBag.Type = type;

        var result = await _api.GetAsync<List<EventModel>>("api/events");
        if (!result.Success)
        {
            if (result.Unauthorised)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Error = result.Error;
            return View(new List<EventModel>());
        }

        var events = (result.Data ?? new List<EventModel>())
            .Where(e => e.EventDate >= DateTime.Today);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            events = events.Where(e =>
                e.EventName.ToLower().Contains(term) ||
                e.Location.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            events = events.Where(e =>
                string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase));
        }

        return View(events.OrderBy(e => e.EventDate).ToList());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}