using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;
using ResultEntity = RaceDay._2API.Models.Result;

namespace RaceDay._2API.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

   
    [Authorize(Roles = "Organiser")]
    [HttpPut("api/enrolments/{id:int}/result")]
    [ProducesResponseType(typeof(ResultResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResultResponseDto>> Capture(int id, ResultRequestDto dto)
    {
        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Participant)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });

        if (enrolment.Event!.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only capture results for your own events." });

        if (enrolment.Status != "Active")
            return Conflict(new { message = "A cancelled enrolment cannot have a result." });

        if (enrolment.Event.EventDate > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "Results can only be captured once the event date has arrived." });

        if (dto.FinishTime == null && dto.Position == null)
            return BadRequest(new { message = "Provide a finish time, a position, or both." });

        if (dto.Position != null && await _db.Results.AnyAsync(r =>
                r.EnrolmentID != id &&
                r.Position == dto.Position &&
                r.Enrolment!.EventID == enrolment.EventID &&
                r.Enrolment.CategoryID == enrolment.CategoryID))
            return Conflict(new { message = "Another participant in this category already has that position." });

        var result = enrolment.Result;
        if (result == null)
        {
            result = new ResultEntity { Enrolment = enrolment };
            _db.Results.Add(result);
        }

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        result.CapturedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(dto.BibNumber))
            enrolment.BibNumber = dto.BibNumber.Trim();
        result.BibNumber = enrolment.BibNumber;

        await _db.SaveChangesAsync();

        return Ok(ToDto(result));
    }

    
    [HttpGet("api/events/{eventId:int}/results")]
    [ProducesResponseType(typeof(IEnumerable<ResultResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ResultResponseDto>>> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var list = await WithDetails()
            .Where(r => r.Enrolment!.EventID == eventId)
            .OrderBy(r => r.Enrolment!.Category!.CategoryName)
            .ThenBy(r => r.Position == null)
            .ThenBy(r => r.Position)
            .ToListAsync();

        return Ok(list.Select(ToDto));
    }

    
    [Authorize(Roles = "Participant")]
    [HttpGet("api/users/me/results")]
    [ProducesResponseType(typeof(IEnumerable<ResultResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ResultResponseDto>>> GetMine()
    {
        var userId = CurrentUserId;
        var list = await WithDetails()
            .Where(r => r.Enrolment!.ParticipantID == userId)
            .OrderByDescending(r => r.Enrolment!.Event!.EventDate)
            .ToListAsync();

        return Ok(list.Select(ToDto));
    }

   
    private IQueryable<ResultEntity> WithDetails() =>
        _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant);

    private static ResultResponseDto ToDto(ResultEntity r) => new()
    {
        ResultsID = r.ResultsID,
        EnrolmentID = r.EnrolmentID,
        EventID = r.Enrolment!.EventID,
        EventName = r.Enrolment.Event?.EventName ?? "",
        EventDate = r.Enrolment.Event?.EventDate ?? default,
        CategoryName = r.Enrolment.Category?.CategoryName ?? "",
        ParticipantName = r.Enrolment.Participant == null
            ? "" : $"{r.Enrolment.Participant.FirstName} {r.Enrolment.Participant.LastName}",
        BibNumber = r.BibNumber,
        FinishTime = r.FinishTime,
        Position = r.Position,
        CapturedAt = r.CapturedAt
    };
}