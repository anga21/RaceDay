using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class EventRequestDto
{
    [Required, MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    // Must be Run, Walk or Cycle
    [Required, RegularExpression("^(Run|Walk|Cycle)$",
        ErrorMessage = "EventType must be Run, Walk or Cycle.")]
    public string EventType { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 1000)]
    public decimal DistanceKm { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}