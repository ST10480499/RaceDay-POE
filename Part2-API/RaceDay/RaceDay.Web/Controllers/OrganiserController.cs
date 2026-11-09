using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[RequireRole("Organiser")]
public class OrganiserController : Controller
{
    private readonly ApiService _api;

    public OrganiserController(ApiService api)
    {
        _api = api;
    }

    // Organiser dashboard: own events, enrolments per event, upcoming dates
    public async Task<IActionResult> Index()
    {
        var model = new OrganiserDashboardViewModel();
        var userId = HttpContext.Session.GetString("UserId");

        var events = await _api.GetAsync<List<EventModel>>("api/events");
        if (!events.Success)
        {
            if (events.Unauthorised)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            model.Error = events.Error;
            return View(model);
        }

        var mine = (events.Data ?? new List<EventModel>())
            .Where(e => e.OrganiserID.ToString() == userId)
            .OrderBy(e => e.EventDate);

        foreach (var e in mine)
        {
            var enrolments = await _api.GetAsync<List<EnrolmentModel>>($"api/events/{e.EventID}/enrolments");

            model.Rows.Add(new DashboardRow
            {
                Event = e,
                EnrolmentCount = enrolments.Success ? (enrolments.Data?.Count ?? 0) : 0
            });
        }

        return View(model);
    }
}