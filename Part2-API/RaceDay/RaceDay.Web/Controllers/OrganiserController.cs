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

    private string? CurrentUserId => HttpContext.Session.GetString("UserId");

    // If the API says our session expired, log out and go to the login page
    private IActionResult? SessionExpired<T>(ApiResponse<T> response)
    {
        if (response.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        return null;
    }

    // ---------- Dashboard ----------

    public async Task<IActionResult> Index()
    {
        var model = new OrganiserDashboardViewModel();

        var events = await _api.GetAsync<List<EventModel>>("api/events");
        if (!events.Success)
        {
            var expired = SessionExpired(events);
            if (expired != null) return expired;

            model.Error = events.Error;
            return View(model);
        }

        var mine = (events.Data ?? new List<EventModel>())
            .Where(e => e.OrganiserID.ToString() == CurrentUserId)
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

    // ---------- Events: list ----------

    public async Task<IActionResult> Events()
    {
        var result = await _api.GetAsync<List<EventModel>>("api/events");
        if (!result.Success)
        {
            var expired = SessionExpired(result);
            if (expired != null) return expired;

            ViewBag.Error = result.Error;
            return View(new List<EventModel>());
        }

        var mine = (result.Data ?? new List<EventModel>())
            .Where(e => e.OrganiserID.ToString() == CurrentUserId)
            .OrderBy(e => e.EventDate)
            .ToList();

        return View(mine);
    }

    // ---------- Events: create ----------

    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View(new EventFormModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventFormModel model, IFormFile? banner)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<EventModel>("api/events", ToRequest(model));
        if (!result.Success)
        {
            var expired = SessionExpired(result);
            if (expired != null) return expired;

            ModelState.AddModelError(string.Empty, result.Error ?? "The event could not be created.");
            return View(model);
        }

        if (banner != null && banner.Length > 0 && result.Data != null)
{
var upload = await _api.UploadFileAsync($"api/events/{result.Data.EventID}/banner", banner);
if (!upload.Success)
{
TempData["Error"] = "Event created, but the banner was not uploaded: " + upload.Error;
return RedirectToAction("Events");
}
}
TempData["Success"] = "Event created.";
        return RedirectToAction("Events");
    }

    // ---------- Events: edit ----------

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var result = await _api.GetAsync<EventModel>($"api/events/{id}");
        if (!result.Success || result.Data == null)
        {
            var expired = SessionExpired(result);
            if (expired != null) return expired;

            TempData["Error"] = result.Error ?? "Event not found.";
            return RedirectToAction("Events");
        }

        if (result.Data.OrganiserID.ToString() != CurrentUserId)
        {
            TempData["Error"] = "You can only edit your own events.";
            return RedirectToAction("Events");
        }

        var e = result.Data;
        return View(new EventFormModel
        {
            EventID = e.EventID,
            EventName = e.EventName,
            Description = e.Description,
            EventDate = e.EventDate,
            Location = e.Location,
            Distance = e.Distance,
            EventType = e.EventType
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditEvent(EventFormModel model, IFormFile? banner)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PutAsync<EventModel>($"api/events/{model.EventID}", ToRequest(model));
        if (!result.Success)
        {
            var expired = SessionExpired(result);
            if (expired != null) return expired;

            ModelState.AddModelError(string.Empty, result.Error ?? "The event could not be updated.");
            return View(model);
        }

        if (banner != null && banner.Length > 0)
{
var upload = await _api.UploadFileAsync($"api/events/{model.EventID}/banner", banner);
if (!upload.Success)
{
TempData["Error"] = "Event updated, but the banner was not uploaded: " + upload.Error;
return RedirectToAction("Events");
}
}
TempData["Success"] = "Event updated.";
        return RedirectToAction("Events");
    }

    // ---------- Events: delete ----------

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _api.DeleteAsync($"api/events/{id}");
        if (!result.Success)
        {
            var expired = SessionExpired(result);
            if (expired != null) return expired;

            TempData["Error"] = result.Error ?? "The event could not be deleted.";
        }
        else
        {
            TempData["Success"] = "Event deleted.";
        }

        return RedirectToAction("Events");
    }

    // ---------- helper ----------

    private static object ToRequest(EventFormModel model)
    {
        return new
        {
            EventName = model.EventName,
            Description = model.Description,
            EventDate = model.EventDate,
            Location = model.Location,
            Distance = model.Distance,
            EventType = model.EventType
        };
    }
}