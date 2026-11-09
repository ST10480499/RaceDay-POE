using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize] // Only logged-in users can use these endpoints
public class FilesController : ControllerBase
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly AppDbContext _context;
    private readonly IFileStorage _storage;

    public FilesController(AppDbContext context, IFileStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Returns an error message if the file is not an acceptable image
    private static string? Validate(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return "Please choose an image to upload.";
        }

        if (file.Length > MaxBytes)
        {
            return "The image must be 5 MB or smaller.";
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/"))
        {
            return "Only JPG, PNG, GIF or WEBP images are allowed.";
        }

        return null;
    }

    private async Task SaveFileAsync(string container, string name, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        await _storage.SaveAsync(container, name, Path.GetExtension(file.FileName).ToLower(), stream);
    }

    private async Task<IActionResult> ServeFileAsync(string container, string name)
    {
        var stored = await _storage.OpenAsync(container, name);
        if (stored == null)
        {
            return NotFound(new { message = "No image has been uploaded yet." });
        }

        return File(stored.Content, stored.ContentType);
    }

    // Uploads the banner image of an event. Only the organiser who owns the event can do this.
    [HttpPost("events/{id}/banner")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var found = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (found == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (found.OrganiserID != GetUserId())
        {
            return Forbid();
        }

        var problem = Validate(file);
        if (problem != null)
        {
            return BadRequest(new { message = problem });
        }

        await SaveFileAsync("banners", id.ToString(), file);
        return Ok(new { message = "Banner uploaded." });
    }

    // Returns the banner image of an event
    [HttpGet("events/{id}/banner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetBanner(int id)
    {
        return ServeFileAsync("banners", id.ToString());
    }

    // Uploads the profile picture of the logged-in user
    [HttpPost("users/me/picture")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPicture(IFormFile file)
    {
        var problem = Validate(file);
        if (problem != null)
        {
            return BadRequest(new { message = problem });
        }

        await SaveFileAsync("pictures", GetUserId().ToString(), file);
        return Ok(new { message = "Profile picture uploaded." });
    }

    // Returns the profile picture of the logged-in user
    [HttpGet("users/me/picture")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetPicture()
    {
        return ServeFileAsync("pictures", GetUserId().ToString());
    }
}
