using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[RequireRole]
public class ImagesController : Controller
{
    private readonly ApiService _api;

    public ImagesController(ApiService api)
    {
        _api = api;
    }

    // Event banner, fetched from the API
    public async Task<IActionResult> Banner(int id)
    {
        var file = await _api.GetFileAsync($"api/events/{id}/banner");
        if (file == null)
        {
            return NotFound();
        }

        return File(file.Value.Bytes, file.Value.ContentType);
    }

    // Profile picture of the logged-in user, fetched from the API
    public async Task<IActionResult> Picture()
    {
        var file = await _api.GetFileAsync("api/users/me/picture");
        if (file == null)
        {
            return NotFound();
        }

        return File(file.Value.Bytes, file.Value.ContentType);
    }

    [HttpPost]
    public async Task<IActionResult> UploadPicture(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please choose an image to upload.";
            return RedirectToAction("Index", "Profile");
        }

        var result = await _api.UploadFileAsync("api/users/me/picture", file);

        if (result.Unauthorised)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "The picture could not be uploaded.";
        }
        else
        {
            TempData["Success"] = "Profile picture updated.";
        }

        return RedirectToAction("Index", "Profile");
    }
}
