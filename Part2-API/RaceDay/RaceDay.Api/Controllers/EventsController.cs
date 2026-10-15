using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize] // Only logged-in users can use these endpoints
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    // The only event types the API accepts
    private static readonly string[] ValidTypes = { "run", "walk", "cycle" };

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    // Reads the logged-in user's id from the session cookie
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Turns an Event from the database into the shape we send back
    private static EventResponse ToResponse(Event e)
    {
        return new EventResponse
        {
            EventID = e.EventID,
            OrganiserID = e.OrganiserID,
            OrganiserName = e.Organiser?.FullName ?? string.Empty,
            EventName = e.EventName,
            Description = e.Description,
            EventDate = e.EventDate,
            Location = e.Location,
            Distance = e.Distance,
            EventType = e.EventType,
            Categories = e.Categories.Select(c => new CategorySummary
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                MaxParticipants = c.MaxParticipants
            }).ToList()
        };
    }

    // Lists all events, soonest first. Both roles can view events.
    [HttpGet]
    [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return Ok(events.Select(ToResponse));
    }

    // Returns one event with its categories
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(int id)
    {
        var found = await _context.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (found == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        return Ok(ToResponse(found));
    }

    // Creates a new event. Organisers only.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateEvent(EventRequest request)
    {
        var type = request.EventType.Trim().ToLower();
        if (!ValidTypes.Contains(type))
        {
            return BadRequest(new { message = "Event type must be run, walk or cycle." });
        }

        var newEvent = new Event
        {
            OrganiserID = GetUserId(),
            EventName = request.EventName.Trim(),
            Description = request.Description.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            Distance = request.Distance,
            EventType = type
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        // Load the event again so the response includes the organiser's name
        var created = await _context.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstAsync(e => e.EventID == newEvent.EventID);

        return CreatedAtAction(nameof(GetEvent), new { id = created.EventID }, ToResponse(created));
    }

    // Updates an event. Only the organiser who created it can do this.
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
    {
        var found = await _context.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (found == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        // An organiser cannot change another organiser's event
        if (found.OrganiserID != GetUserId())
        {
            return Forbid();
        }

        var type = request.EventType.Trim().ToLower();
        if (!ValidTypes.Contains(type))
        {
            return BadRequest(new { message = "Event type must be run, walk or cycle." });
        }

        found.EventName = request.EventName.Trim();
        found.Description = request.Description.Trim();
        found.EventDate = request.EventDate;
        found.Location = request.Location.Trim();
        found.Distance = request.Distance;
        found.EventType = type;

        await _context.SaveChangesAsync();

        return Ok(ToResponse(found));
    }

    // Deletes an event and its categories. Only the organiser who created it can do this.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
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

        _context.Events.Remove(found);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Event deleted." });
    }
}