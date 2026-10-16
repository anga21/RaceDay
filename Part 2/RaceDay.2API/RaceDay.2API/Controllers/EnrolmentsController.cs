using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;
using EnrolmentEntity = RaceDay._2API.Models.Enrolment;

namespace RaceDay._2API.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    
    [Authorize(Roles = "Participant")]
    [HttpPost("api/events/{eventId:int}/enrolments")]
    [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnrolmentResponseDto>> Enrol(int eventId, EnrolmentRequestDto dto)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.EventDate < DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "This event has already taken place." });

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryID && c.EventID == eventId);
        if (category == null)
            return BadRequest(new { message = "That category does not belong to this event." });

        var userId = CurrentUserId;

        if (await _db.Enrolments.AnyAsync(e =>
                e.EventID == eventId && e.ParticipantID == userId && e.Status == "Active"))
            return Conflict(new { message = "You are already enrolled in this event." });

        
        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(e =>
            e.EventID == eventId && e.ParticipantID == userId && e.CategoryID == dto.CategoryID);

        if (enrolment != null)
        {
            enrolment.Status = "Active";
            enrolment.EnrolmentDate = DateTime.Now;
        }
        else
        {
            enrolment = new EnrolmentEntity
            {
                EventID = eventId,
                ParticipantID = userId,
                CategoryID = dto.CategoryID,
                Status = "Active"
            };
            _db.Enrolments.Add(enrolment);
        }

        await _db.SaveChangesAsync();

        var saved = await WithDetails().FirstAsync(e => e.EnrolmentID == enrolment.EnrolmentID);
        return StatusCode(StatusCodes.Status201Created, ToDto(saved));
    }

    
    [Authorize(Roles = "Participant")]
    [HttpGet("api/users/me/enrolments")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetMine()
    {
        var userId = CurrentUserId;
        var list = await WithDetails()
            .Where(e => e.ParticipantID == userId)
            .OrderByDescending(e => e.EnrolmentDate)
            .ToListAsync();

        return Ok(list.Select(ToDto));
    }

    
    [Authorize(Roles = "Organiser")]
    [HttpGet("api/events/{eventId:int}/enrolments")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetForEvent(int eventId)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only view enrolments for your own events." });

        var list = await WithDetails()
            .Where(e => e.EventID == eventId)
            .OrderBy(e => e.EnrolmentDate)
            .ToListAsync();

        return Ok(list.Select(ToDto));
    }

    
    [Authorize(Roles = "Participant")]
    [HttpDelete("api/enrolments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });

        if (enrolment.ParticipantID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only cancel your own enrolments." });

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == id))
            return Conflict(new { message = "This enrolment already has a result and cannot be cancelled." });

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }

    
    private IQueryable<EnrolmentEntity> WithDetails() =>
        _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Participant);

    private static EnrolmentResponseDto ToDto(EnrolmentEntity e) => new()
    {
        EnrolmentID = e.EnrolmentID,
        EventID = e.EventID,
        EventName = e.Event?.EventName ?? "",
        CategoryID = e.CategoryID,
        CategoryName = e.Category?.CategoryName ?? "",
        ParticipantID = e.ParticipantID,
        ParticipantName = e.Participant == null ? "" : $"{e.Participant.FirstName} {e.Participant.LastName}",
        BibNumber = e.BibNumber,
        EnrolmentDate = e.EnrolmentDate,
        Status = e.Status
    };
}