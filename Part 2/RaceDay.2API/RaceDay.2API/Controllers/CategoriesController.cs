
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;
using CategoryEntity = RaceDay._2API.Models.Category;

namespace RaceDay._2API.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    
    [HttpGet("api/events/{eventId:int}/categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var categories = await _db.Categories
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return Ok(categories.Select(ToDto));
    }

    
    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/categories")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponseDto>> Create(int eventId, CategoryRequestDto dto)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only add categories to your own events." });

        var name = dto.CategoryName.Trim();
        if (await _db.Categories.AnyAsync(c => c.EventID == eventId && c.CategoryName == name))
            return Conflict(new { message = "This event already has a category with that name." });

        var category = new CategoryEntity
        {
            EventID = eventId,
            CategoryName = name,
            Description = dto.Description?.Trim()
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToDto(category));
    }

    
    [Authorize(Roles = "Organiser")]
    [HttpPut("api/categories/{id:int}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponseDto>> Update(int id, CategoryRequestDto dto)
    {
        var category = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound(new { message = "Category not found." });

        if (category.Event!.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only change categories on your own events." });

        var name = dto.CategoryName.Trim();
        if (await _db.Categories.AnyAsync(c =>
                c.EventID == category.EventID && c.CategoryName == name && c.CategoryID != id))
            return Conflict(new { message = "This event already has a category with that name." });

        category.CategoryName = name;
        category.Description = dto.Description?.Trim();
        await _db.SaveChangesAsync();

        return Ok(ToDto(category));
    }

    
    [Authorize(Roles = "Organiser")]
    [HttpDelete("api/categories/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound(new { message = "Category not found." });

        if (category.Event!.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only delete categories on your own events." });

        if (await _db.Enrolments.AnyAsync(en => en.CategoryID == id))
            return Conflict(new { message = "This category has enrolments and cannot be deleted." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static CategoryResponseDto ToDto(CategoryEntity c) => new()
    {
        CategoryID = c.CategoryID,
        EventID = c.EventID,
        CategoryName = c.CategoryName,
        Description = c.Description
    };
}