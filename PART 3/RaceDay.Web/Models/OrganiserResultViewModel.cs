namespace RaceDay.Web.Models;

public class OrganiserResultViewModel
{
    public int EnrolmentId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public int Position { get; set; }

    public string FinishTime { get; set; } = string.Empty;

    public int BibNumber { get; set; }
}