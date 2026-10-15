using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize] // Only logged-in users can use these endpoints
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // Reads the logged-in user's id from the session cookie
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Turns a Category from the database into the shape we send back
    private static CategoryResponse ToResponse(Category c)
    {
        return new CategoryResponse
        {
            CategoryID = c.CategoryID,
            EventID = c.EventID,
            CategoryName = c.CategoryName,
            MaxParticipants = c.MaxParticipants
        };
    }

    // Lists the categories of one event. Both roles can view them.
    [HttpGet("events/{eventId}/categories")]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        if (!await _context.Events.AnyAsync(e => e.EventID == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var categories = await _context.Categories
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return Ok(categories.Select(ToResponse));
    }

    // Adds a category to an event. Only the organiser who owns the event can do this.
    [HttpPost("events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategory(int eventId, CategoryRequest request)
    {
        var found = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

        if (found == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        // An organiser can only add categories to their own events
        if (found.OrganiserID != GetUserId())
        {
            return Forbid();
        }

        var name = request.CategoryName.Trim();

        // Two categories in the same event cannot share a name
        if (await _context.Categories.AnyAsync(c => c.EventID == eventId && c.CategoryName.ToLower() == name.ToLower()))
        {
            return Conflict(new { message = "This event already has a category with that name." });
        }

        var category = new Category
        {
            EventID = eventId,
            CategoryName = name,
            MaxParticipants = request.MaxParticipants
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResponse(category));
    }

    // Updates a category. Only the organiser who owns the event can do this.
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var found = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (found == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (found.Event!.OrganiserID != GetUserId())
        {
            return Forbid();
        }

        var name = request.CategoryName.Trim();

        // The new name must not clash with another category in the same event
        if (await _context.Categories.AnyAsync(c =>
                c.EventID == found.EventID && c.CategoryID != id && c.CategoryName.ToLower() == name.ToLower()))
        {
            return Conflict(new { message = "This event already has a category with that name." });
        }

        found.CategoryName = name;
        found.MaxParticipants = request.MaxParticipants;
        await _context.SaveChangesAsync();

        return Ok(ToResponse(found));
    }

    // Deletes a category. Only the organiser who owns the event can do this.
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var found = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (found == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (found.Event!.OrganiserID != GetUserId())
        {
            return Forbid();
        }

        _context.Categories.Remove(found);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Category deleted." });
    }
}