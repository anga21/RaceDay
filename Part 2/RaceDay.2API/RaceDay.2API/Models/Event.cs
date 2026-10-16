using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay._2API.Models;

[Table("Event")]
public class Event
{
    [Key]
    public int EventID { get; set; }

    public int UserID { get; set; }

    [Required, MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    // New in Part 2: Run, Walk or Cycle
    [Required, MaxLength(10)]
    public string EventType { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    // New in Part 2
    [Column(TypeName = "decimal(6,2)")]
    public decimal DistanceKm { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? Organiser { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Route> Routes { get; set; } = new List<Route>();
    public ICollection<WeatherForecast> WeatherForecasts { get; set; } = new List<WeatherForecast>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}