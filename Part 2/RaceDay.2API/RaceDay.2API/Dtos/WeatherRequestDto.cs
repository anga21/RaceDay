using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class WeatherRequestDto
{
    // Format yyyy-MM-dd
    public DateOnly ForecastDate { get; set; }

    [MaxLength(100)]
    public string? Conditions { get; set; }

    // Degrees Celsius
    [Range(-50, 60)]
    public decimal? Temperature { get; set; }

    // Kilometres per hour
    [Range(0, 400)]
    public decimal? WindSpeed { get; set; }
}