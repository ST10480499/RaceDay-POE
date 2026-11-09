using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[RequireRole("Organiser")]
public class CategoriesController : Controller
{
    private readonly ApiService _api;

    public CategoriesController(ApiService api)
    {
        _api = api;
    }

    private string? CurrentUserId => HttpContext.Session.GetString("UserId");

    // Loads an event and checks that it belongs to the logged-in organiser
    private async Task<(EventModel? Event, IActionResult? Redirect)> LoadOwnEventAsync(int eventId)
    {
        var result = await _api.GetAsync<EventModel>($"api/events/{eventId}");

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return (null, RedirectToAction("Login", "Account"));
        }

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Event not found.";
            return (null, RedirectToAction("Events", "Organiser"));
        }

        if (result.Data.OrganiserID.ToString() != CurrentUserId)
        {
            TempData["Error"] = "You can only manage categories for your own events.";
            return (null, RedirectToAction("Events", "Organiser"));
        }

        return (result.Data, null);
    }

    // ---------- List categories and show the add form ----------

    public async Task<IActionResult> Index(int eventId)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        ViewBag.Event = ev;
        return View(new CategoryFormModel { EventID = eventId });
    }

    // ---------- Add ----------

    [HttpPost]
    public async Task<IActionResult> Add(CategoryFormModel model)
    {
        var (ev, redirect) = await LoadOwnEventAsync(model.EventID);
        if (redirect != null) return redirect;

        if (!ModelState.IsValid)
        {
            ViewBag.Event = ev;
            return View("Index", model);
        }

        var result = await _api.PostAsync<CategoryModel>(
            $"api/events/{model.EventID}/categories",
            new { CategoryName = model.CategoryName, MaxParticipants = model.MaxParticipants });

        if (!result.Success)
        {
            if (result.Unauthorised)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "The category could not be added.");
            ViewBag.Event = ev;
            return View("Index", model);
        }

        TempData["Success"] = "Category added.";
        return RedirectToAction("Index", new { eventId = model.EventID });
    }

    // ---------- Edit ----------

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int eventId)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        var category = ev!.Categories.FirstOrDefault(c => c.CategoryID == id);
        if (category == null)
        {
            TempData["Error"] = "Category not found.";
            return RedirectToAction("Index", new { eventId });
        }

        ViewBag.Event = ev;
        return View(new CategoryFormModel
        {
            CategoryID = category.CategoryID,
            EventID = eventId,
            CategoryName = category.CategoryName,
            MaxParticipants = category.MaxParticipants
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoryFormModel model)
    {
        var (ev, redirect) = await LoadOwnEventAsync(model.EventID);
        if (redirect != null) return redirect;

        if (!ModelState.IsValid)
        {
            ViewBag.Event = ev;
            return View(model);
        }

        var result = await _api.PutAsync<CategoryModel>(
            $"api/categories/{model.CategoryID}",
            new { CategoryName = model.CategoryName, MaxParticipants = model.MaxParticipants });

        if (!result.Success)
        {
            if (result.Unauthorised)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "The category could not be updated.");
            ViewBag.Event = ev;
            return View(model);
        }

        TempData["Success"] = "Category updated.";
        return RedirectToAction("Index", new { eventId = model.EventID });
    }

    // ---------- Delete ----------

    [HttpPost]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        var result = await _api.DeleteAsync($"api/categories/{id}");
        if (!result.Success)
        {
            if (result.Unauthorised)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            TempData["Error"] = result.Error ?? "The category could not be deleted.";
        }
        else
        {
            TempData["Success"] = "Category deleted.";
        }

        return RedirectToAction("Index", new { eventId });
    }
}