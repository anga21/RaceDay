using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;
using RouteEntity = RaceDay._2API.Models.Route;

namespace RaceDay._2API.Controllers;

[ApiController]
public class RoutesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public RoutesController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lists the routes for an event, so participants can prepare for race day. Open to everyone.</summary>
    [HttpGet("api/events/{eventId:int}/routes")]
    [ProducesResponseType(typeof(IEnumerable<RouteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RouteResponseDto>>> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var routes = await _db.Routes
            .Where(r => r.EventID == eventId)
            .OrderBy(r => r.DistanceKm)
            .ToListAsync();

        return Ok(routes.Select(ToDto));
    }

    /// <summary>Adds a route to an event. Only the Organiser who owns the event can do this.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/routes")]
    [ProducesResponseType(typeof(RouteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RouteResponseDto>> Create(int eventId, RouteRequestDto dto)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only add routes to your own events." });

        var name = dto.RouteName.Trim();
        if (await _db.Routes.AnyAsync(r => r.EventID == eventId && r.RouteName == name))
            return Conflict(new { message = "This event already has a route with that name." });

        var route = new RouteEntity
        {
            EventID = eventId,
            RouteName = name,
            StartPoint = dto.StartPoint.Trim(),
            EndPoint = dto.EndPoint.Trim(),
            DistanceKm = dto.DistanceKm,
            Description = dto.Description?.Trim()
        };

        _db.Routes.Add(route);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToDto(route));
    }

    private static RouteResponseDto ToDto(RouteEntity r) => new()
    {
        RouteID = r.RouteID,
        EventID = r.EventID,
        RouteName = r.RouteName,
        StartPoint = r.StartPoint,
        EndPoint = r.EndPoint,
        DistanceKm = r.DistanceKm,
        Description = r.Description
    };
}