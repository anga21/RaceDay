namespace RaceDay._2API.Dtos;

public class EventResponseDto
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string? Description { get; set; }
}