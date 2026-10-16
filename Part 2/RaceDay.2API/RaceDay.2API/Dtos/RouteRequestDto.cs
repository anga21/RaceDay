using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class RouteRequestDto
{
    [Required, MaxLength(100)]
    public string RouteName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string StartPoint { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string EndPoint { get; set; } = string.Empty;

    [Range(0.01, 1000)]
    public decimal DistanceKm { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
