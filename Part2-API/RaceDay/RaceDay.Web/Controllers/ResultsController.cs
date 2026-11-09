using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ResultsController : Controller
{
    private readonly ApiService _api;

    public ResultsController(ApiService api)
    {
        _api = api;
    }

    private string? CurrentUserId => HttpContext.Session.GetString("UserId");

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
            TempData["Error"] = "You can only capture results for your own events.";
            return RedirectToAction("Events", "Organiser");
        }

        var enrolments = await _api.GetAsync<List<EnrolmentModel>>($"api/events/{eventId}/enrolments");
        var results = await _api.GetAsync<List<ResultModel>>($"api/events/{eventId}/results");

        ViewBag.Error = enrolments.Success ? null : enrolments.Error;

        return View(new EventResultsViewModel
        {
            Event = ev.Data,
            Enrolments = enrolments.Data ?? new List<EnrolmentModel>(),
            Results = results.Data ?? new List<ResultModel>()
        });
    }

    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Save(int enrolmentId, int eventId, string finishTime, int position)
    {
        var result = await _api.PostAsync<ResultModel>(
            $"api/enrolments/{enrolmentId}/result",
            new { FinishTime = finishTime, Position = position });

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "The result could not be saved.";
        }
        else
        {
            TempData["Success"] = "Result saved.";
        }

        return RedirectToAction("Event", new { eventId });
    }

    [RequireRole("Participant")]
    public async Task<IActionResult> Mine()
    {
        var mine = await _api.GetAsync<List<ResultModel>>("api/users/me/results");

        if (mine.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        var data = mine.Data ?? new List<ResultModel>();

        var events = await _api.GetAsync<List<EventModel>>("api/events");
        var dates = new Dictionary<int, DateTime>();
        foreach (var e in events.Data ?? new List<EventModel>())
        {
            dates[e.EventID] = e.EventDate;
        }

        var finishers = new Dictionary<int, List<ResultModel>>();
        foreach (var id in data.Select(r => r.EventID).Distinct())
        {
            var all = await _api.GetAsync<List<ResultModel>>($"api/events/{id}/results");
            finishers[id] = all.Data ?? new List<ResultModel>();
        }

        var rows = new List<ResultRow>();
        foreach (var r in data)
        {
            rows.Add(new ResultRow
            {
                Result = r,
                EventDate = dates.TryGetValue(r.EventID, out var d) ? d : DateTime.MinValue,
                Finishers = finishers[r.EventID].Count(x => x.CategoryID == r.CategoryID)
            });
        }

        ViewBag.Error = mine.Success ? null : mine.Error;
        return View(rows);
    }
}
