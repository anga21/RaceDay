namespace RaceDay._2API.Dtos;

public class ResultResponseDto
{
    public int ResultsID { get; set; }
    public int EnrolmentID { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string? BibNumber { get; set; }
    public TimeOnly? FinishTime { get; set; }
    public int? Position { get; set; }
    public DateTime CapturedAt { get; set; }
}