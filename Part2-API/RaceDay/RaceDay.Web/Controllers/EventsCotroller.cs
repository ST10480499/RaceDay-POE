using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[RequireRole("Participant")]
public class EventsController : Controller
{
    private readonly ApiService _api;

    public EventsController(ApiService api)
    {
        _api = api;
    }

    // Full event information and its categories
    public async Task<IActionResult> Details(int id)
    {
        var result = await _api.GetAsync<EventModel>($"api/events/{id}");

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Event not found.";
            return RedirectToAction("Index", "Home");
        }

        return View(result.Data);
    }

    // Enrol the logged-in participant in a category
    [HttpPost]
    public async Task<IActionResult> Enrol(int categoryId, int eventId)
    {
        var result = await _api.PostAsync<EnrolmentModel>($"api/categories/{categoryId}/enrol");

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "We could not enrol you. Please try again.";
            return RedirectToAction("Details", new { id = eventId });
        }

        TempData["Success"] = "You are enrolled!";
        return RedirectToAction("Mine", "Enrolments");
    }
}