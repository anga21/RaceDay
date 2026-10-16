using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay._2API.Models;

[Table("Route")]
public class Route
{
    [Key]
    public int RouteID { get; set; }

    public int EventID { get; set; }

    [Required, MaxLength(100)]
    public string RouteName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string StartPoint { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string EndPoint { get; set; } = string.Empty;

    [Column(TypeName = "decimal(6,2)")]
    public decimal DistanceKm { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [ForeignKey(nameof(EventID))]
    public Event? Event { get; set; }
}