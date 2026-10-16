namespace RaceDay._2API.Dtos;

public class WeatherResponseDto
{
    public int WeatherForecastID { get; set; }
    public int EventID { get; set; }
    public DateOnly ForecastDate { get; set; }
    public string? Conditions { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? WindSpeed { get; set; }
}