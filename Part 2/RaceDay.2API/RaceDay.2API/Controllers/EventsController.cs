using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;
using EventEntity = RaceDay._2API.Models.Event;

namespace RaceDay._2API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lists upcoming events, optionally filtered by type (Run, Walk or Cycle).</summary>
    /// <remarks>Past events are hidden unless includePast is true. Open to everyone.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAll(
        [FromQuery] string? eventType, [FromQuery] bool includePast = false)
    {
        var query = _db.Events.Include(e => e.Organiser).AsQueryable();

        if (!includePast)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            query = query.Where(e => e.EventDate >= today);
        }
        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => e.EventType == eventType);

        var events = await query
            .OrderBy(e => e.EventDate).ThenBy(e => e.StartTime)
            .ToListAsync();

        return Ok(events.Select(ToDto));
    }

    /// <summary>Returns the details of one event. Open to everyone.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponseDto>> GetById(int id)
    {
        var ev = await _db.Events.Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound(new { message = "Event not found." });
        return Ok(ToDto(ev));
    }

    /// <summary>Creates a new event owned by the logged-in Organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventResponseDto>> Create(EventRequestDto dto)
    {
        if (dto.EndTime != null && dto.EndTime <= dto.StartTime)
            return BadRequest(new { message = "EndTime must be after StartTime." });

        var ev = new EventEntity { UserID = CurrentUserId };
        Apply(ev, dto);

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventID }, ToDto(ev));
    }

    /// <summary>Updates an event. Only the Organiser who created it can do this.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponseDto>> Update(int id, EventRequestDto dto)
    {
        var ev = await _db.Events.Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only change your own events." });

        if (dto.EndTime != null && dto.EndTime <= dto.StartTime)
            return BadRequest(new { message = "EndTime must be after StartTime." });

        Apply(ev, dto);
        await _db.SaveChangesAsync();
        return Ok(ToDto(ev));
    }

    /// <summary>Deletes an event and its categories, routes and forecasts.</summary>
    /// <remarks>Only the Organiser who created it can do this. Events that already have enrolments cannot be deleted.</remarks>
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Categories)
            .Include(e => e.Routes)
            .Include(e => e.WeatherForecasts)
            .FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only delete your own events." });

        if (await _db.Enrolments.AnyAsync(en => en.EventID == id))
            return Conflict(new { message = "This event has enrolments and cannot be deleted." });

        _db.Categories.RemoveRange(ev.Categories);
        _db.Routes.RemoveRange(ev.Routes);
        _db.WeatherForecasts.RemoveRange(ev.WeatherForecasts);
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static void Apply(EventEntity ev, EventRequestDto dto)
    {
        ev.EventName = dto.EventName.Trim();
        ev.EventType = dto.EventType;
        ev.EventDate = dto.EventDate;
        ev.StartTime = dto.StartTime;
        ev.EndTime = dto.EndTime;
        ev.Location = dto.Location.Trim();
        ev.DistanceKm = dto.DistanceKm;
        ev.Description = dto.Description?.Trim();
    }

    private static EventResponseDto ToDto(EventEntity e) => new()
    {
        EventID = e.EventID,
        OrganiserID = e.UserID,
        OrganiserName = e.Organiser == null ? "" : $"{e.Organiser.FirstName} {e.Organiser.LastName}",
        EventName = e.EventName,
        EventType = e.EventType,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        Description = e.Description
    };
}