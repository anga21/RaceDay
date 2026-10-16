using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay._2API.Models;

[Table("Enrolments")]
public class Enrolment
{
    [Key]
    public int EnrolmentID { get; set; }

    public int EventID { get; set; }
    public int ParticipantID { get; set; }
    public int CategoryID { get; set; }

    [MaxLength(10)]
    public string? BibNumber { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.Now;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";

    [ForeignKey(nameof(EventID))]
    public Event? Event { get; set; }

    [ForeignKey(nameof(ParticipantID))]
    public User? Participant { get; set; }

    [ForeignKey(nameof(CategoryID))]
    public Category? Category { get; set; }

    public Result? Result { get; set; }
}