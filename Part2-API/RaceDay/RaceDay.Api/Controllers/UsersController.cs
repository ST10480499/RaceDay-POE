using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // Reads the logged-in user's id from the session cookie
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Returns the profile of the user who is currently logged in
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user.UserID,
            user.FullName,
            user.Email,
            Role = user.Role!.RoleName,
            user.CreatedDate
        });
    }

    // Updates the name and email of the user who is currently logged in
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return Unauthorized();
        }

        var email = request.Email.Trim().ToLower();

        // The new email must not belong to another user
        if (await _context.Users.AnyAsync(u => u.Email == email && u.UserID != userId))
        {
            return Conflict(new { message = "That email is already registered." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.UserID,
            user.FullName,
            user.Email,
            Role = user.Role!.RoleName
        });
    }
}