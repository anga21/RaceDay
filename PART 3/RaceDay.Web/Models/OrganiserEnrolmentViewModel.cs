namespace RaceDay.Web.Models;

public class OrganiserEnrolmentViewModel
{
    public int EnrolmentId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}