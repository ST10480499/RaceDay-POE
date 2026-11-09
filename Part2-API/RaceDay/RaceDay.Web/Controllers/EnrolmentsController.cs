using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EnrolmentsController : Controller
{
    private readonly ApiService _api;

    public EnrolmentsController(ApiService api)
    {
        _api = api;
    }

    private string? CurrentUserId => HttpContext.Session.GetString("UserId");

    // Organiser: see everyone enrolled in one of their events
    [RequireRole("Organiser")]
    public async Task<IActionResult> Event(int eventId)
    {
        var ev = await _api.GetAsync<EventModel>($"api/events/{eventId}");

        if (ev.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!ev.Success || ev.Data == null)
        {
            TempData["Error"] = ev.Error ?? "Event not found.";
            return RedirectToAction("Events", "Organiser");
        }

        if (ev.Data.OrganiserID.ToString() != CurrentUserId)
        {
            TempData["Error"] = "You can only view enrolments for your own events.";
            return RedirectToAction("Events", "Organiser");
        }

        var enrolments = await _api.GetAsync<List<EnrolmentModel>>($"api/events/{eventId}/enrolments");

        if (enrolments.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        ViewBag.Event = ev.Data;
        ViewBag.Error = enrolments.Success ? null : enrolments.Error;

        return View(enrolments.Data ?? new List<EnrolmentModel>());
    }

    // Participant: see all the events they have enrolled for
    [RequireRole("Participant")]
    public async Task<IActionResult> Mine()
    {
        var result = await _api.GetAsync<List<EnrolmentModel>>("api/users/me/enrolments");

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        ViewBag.Error = result.Success ? null : result.Error;

        return View(result.Data ?? new List<EnrolmentModel>());
    }
}