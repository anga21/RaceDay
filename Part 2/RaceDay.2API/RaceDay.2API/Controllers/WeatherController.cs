using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;
using WeatherEntity = RaceDay._2API.Models.WeatherForecast;

namespace RaceDay._2API.Controllers;

[ApiController]
public class WeatherController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public WeatherController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns the weather forecast entries for an event, earliest date first. Open to everyone.</summary>
    [HttpGet("api/events/{eventId:int}/weather")]
    [ProducesResponseType(typeof(IEnumerable<WeatherResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<WeatherResponseDto>>> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var forecasts = await _db.WeatherForecasts
            .Where(w => w.EventID == eventId)
            .OrderBy(w => w.ForecastDate)
            .ToListAsync();

        return Ok(forecasts.Select(ToDto));
    }

    /// <summary>Adds a forecast entry for an event, or updates the entry that already exists for that date.</summary>
    /// <remarks>Only the Organiser who owns the event can do this. Returns 201 for a new entry and 200 for an update.</remarks>
    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/weather")]
    [ProducesResponseType(typeof(WeatherResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WeatherResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherResponseDto>> AddOrUpdate(int eventId, WeatherRequestDto dto)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only add forecasts to your own events." });

        if (string.IsNullOrWhiteSpace(dto.Conditions) && dto.Temperature == null && dto.WindSpeed == null)
            return BadRequest(new { message = "Provide conditions, temperature, wind speed, or a combination." });

        var forecast = await _db.WeatherForecasts.FirstOrDefaultAsync(w =>
            w.EventID == eventId && w.ForecastDate == dto.ForecastDate);

        if (forecast != null)
        {
            forecast.Conditions = dto.Conditions?.Trim();
            forecast.Temperature = dto.Temperature;
            forecast.WindSpeed = dto.WindSpeed;
            await _db.SaveChangesAsync();
            return Ok(ToDto(forecast));
        }

        forecast = new WeatherEntity
        {
            EventID = eventId,
            ForecastDate = dto.ForecastDate,
            Conditions = dto.Conditions?.Trim(),
            Temperature = dto.Temperature,
            WindSpeed = dto.WindSpeed
        };
        _db.WeatherForecasts.Add(forecast);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToDto(forecast));
    }

    private static WeatherResponseDto ToDto(WeatherEntity w) => new()
    {
        WeatherForecastID = w.WeatherForecastID,
        EventID = w.EventID,
        ForecastDate = w.ForecastDate,
        Conditions = w.Conditions,
        Temperature = w.Temperature,
        WindSpeed = w.WindSpeed
    };
}