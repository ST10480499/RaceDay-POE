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
public class ResultsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResultsController(AppDbContext context)
    {
        _context = context;
    }

    // Reads the logged-in user's id from the session cookie
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Turns a Result from the database into the shape we send back
    private static ResultResponse ToResponse(Result r)
    {
        return new ResultResponse
        {
            ResultID = r.ResultID,
            EnrolmentID = r.EnrolmentID,
            ParticipantID = r.Enrolment!.ParticipantID,
            ParticipantName = r.Enrolment.Participant?.FullName ?? string.Empty,
            EventID = r.Enrolment.Category!.EventID,
            EventName = r.Enrolment.Category.Event?.EventName ?? string.Empty,
            CategoryID = r.Enrolment.CategoryID,
            CategoryName = r.Enrolment.Category.CategoryName,
            FinishTime = r.FinishTime.ToString(@"hh\:mm\:ss"),
            Position = r.Position
        };
    }

    // Records the finish time and position for an enrolment. Only the organiser who owns the event can do this.
    [HttpPost("enrolments/{id}/result")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CaptureResult(int id, ResultRequest request)
    {
        var enrolment = await _context.Enrolments
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment == null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        // An organiser can only record results for their own events
        if (enrolment.Category!.Event!.OrganiserID != GetUserId())
        {
            return Forbid();
        }

        // The finish time must be a valid time, such as 01:45:30
        if (!TimeSpan.TryParse(request.FinishTime, out var finishTime) || finishTime <= TimeSpan.Zero)
        {
            return BadRequest(new { message = "Finish time must be a valid time like 01:45:30." });
        }

        // Each enrolment can only have one result
        if (await _context.Results.AnyAsync(r => r.EnrolmentID == id))
        {
            return Conflict(new { message = "A result has already been recorded for this enrolment." });
        }

        var result = new Result
        {
            EnrolmentID = id,
            FinishTime = finishTime,
            Position = request.Position
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        // Load it again so the response includes the names
        var saved = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .FirstAsync(r => r.ResultID == result.ResultID);

        return StatusCode(StatusCodes.Status201Created, ToResponse(saved));
    }

    // Lists the results of the logged-in participant. Participants only.
    [HttpGet("users/me/results")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyResults()
    {
        var userId = GetUserId();

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .Where(r => r.Enrolment!.ParticipantID == userId)
            .OrderBy(r => r.Enrolment!.Category!.Event!.EventDate)
            .ToListAsync();

        return Ok(results.Select(ToResponse));
    }

    // Lists every result for one event, grouped by category and ordered by position.
    [HttpGet("events/{eventId}/results")]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        if (!await _context.Events.AnyAsync(e => e.EventID == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .Where(r => r.Enrolment!.Category!.EventID == eventId)
            .OrderBy(r => r.Enrolment!.Category!.CategoryName)
            .ThenBy(r => r.Position)
            .ToListAsync();

        return Ok(results.Select(ToResponse));
    }
}