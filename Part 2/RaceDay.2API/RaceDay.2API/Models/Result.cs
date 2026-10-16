using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay._2API.Models;

[Table("Results")]
public class Result
{
    [Key]
    public int ResultsID { get; set; }

    public int EnrolmentID { get; set; }

    public TimeOnly? FinishTime { get; set; }

    [MaxLength(10)]
    public string? BibNumber { get; set; }

    public int? Position { get; set; }

    public DateTime CapturedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(EnrolmentID))]
    public Enrolment? Enrolment { get; set; }
}