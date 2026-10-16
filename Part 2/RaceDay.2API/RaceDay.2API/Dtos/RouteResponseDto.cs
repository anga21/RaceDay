namespace RaceDay._2API.Dtos;

public class RouteResponseDto
{
    public int RouteID { get; set; }
    public int EventID { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string? Description { get; set; }
}