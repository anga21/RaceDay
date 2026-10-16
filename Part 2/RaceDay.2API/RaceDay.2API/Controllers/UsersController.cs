using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;
using UserEntity = RaceDay._2API.Models.User;

namespace RaceDay._2API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/me")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    
    [HttpGet]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileResponseDto>> GetMe()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return Unauthorized();
        return Ok(ToDto(user));
    }

    
    
    [HttpPut]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileResponseDto>> UpdateMe(UpdateProfileDto dto)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return Unauthorized();

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        await _db.SaveChangesAsync();

        return Ok(ToDto(user));
    }

    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteMe()
    {
        var userId = CurrentUserId;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        if (await _db.Events.AnyAsync(e => e.UserID == userId) ||
            await _db.Enrolments.AnyAsync(e => e.ParticipantID == userId))
            return Conflict(new
            {
                message = "This account has events or enrolments and cannot be deleted."
            });

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    private static ProfileResponseDto ToDto(UserEntity u) => new()
    {
        UserID = u.UserID,
        FirstName = u.FirstName,
        LastName = u.LastName,
        PhoneNumber = u.PhoneNumber,
        Email = u.Email,
        Role = u.Role,
        CreatedAt = u.CreatedAt
    };
}