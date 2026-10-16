using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay._2API.Models;

[Table("WeatherForecast")]
public class WeatherForecast
{
    [Key]
    public int WeatherForecastID { get; set; }

    public int EventID { get; set; }

    public DateOnly ForecastDate { get; set; }

    [MaxLength(100)]
    public string? Conditions { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal? Temperature { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal? WindSpeed { get; set; }

    [ForeignKey(nameof(EventID))]
    public Event? Event { get; set; }
}