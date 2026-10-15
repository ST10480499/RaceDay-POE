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
public class EnrolmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnrolmentsController(AppDbContext context)
    {
        _context = context;
    }

    // Reads the logged-in user's id from the session cookie
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Turns an Enrolment from the database into the shape we send back
    private static EnrolmentResponse ToResponse(Enrolment e)
    {
        return new EnrolmentResponse
        {
            EnrolmentID = e.EnrolmentID,
            ParticipantID = e.ParticipantID,
            ParticipantName = e.Participant?.FullName ?? string.Empty,
            ParticipantEmail = e.Participant?.Email ?? string.Empty,
            EventID = e.Category!.EventID,
            EventName = e.Category.Event?.EventName ?? string.Empty,
            CategoryID = e.CategoryID,
            CategoryName = e.Category.CategoryName,
            EnrolmentDate = e.EnrolmentDate
        };
    }

    // Enrols the logged-in participant in a category. Participants only.
    [HttpPost("categories/{id}/enrol")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        var userId = GetUserId();

        // A participant can only enrol once in the same category
        if (await _context.Enrolments.AnyAsync(e => e.CategoryID == id && e.ParticipantID == userId))
        {
            return Conflict(new { message = "You are already enrolled in this category." });
        }

        // The category stops accepting entries once it is full
        var enrolled = await _context.Enrolments.CountAsync(e => e.CategoryID == id);
        if (enrolled >= category.MaxParticipants)
        {
            return Conflict(new { message = "This category is full." });
        }

        var enrolment = new Enrolment
        {
            ParticipantID = userId,
            CategoryID = id
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        // Load it again so the response includes the names
        var saved = await _context.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .FirstAsync(e => e.EnrolmentID == enrolment.EnrolmentID);

        return StatusCode(StatusCodes.Status201Created, ToResponse(saved));
    }

    // Lists the enrolments of the logged-in participant. Participants only.
    [HttpGet("users/me/enrolments")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var userId = GetUserId();

        var enrolments = await _context.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Where(e => e.ParticipantID == userId)
            .OrderBy(e => e.Category!.Event!.EventDate)
            .ToListAsync();

        return Ok(enrolments.Select(ToResponse));
    }

    // Lists every enrolment for one event. Only the organiser who owns the event can do this.
    [HttpGet("events/{eventId}/enrolments")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var found = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

        if (found == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (found.OrganiserID != GetUserId())
        {
            return Forbid();
        }

        var enrolments = await _context.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Where(e => e.Category!.EventID == eventId)
            .OrderBy(e => e.Category!.CategoryName)
            .ThenBy(e => e.EnrolmentDate)
            .ToListAsync();

        return Ok(enrolments.Select(ToResponse));
    }
}