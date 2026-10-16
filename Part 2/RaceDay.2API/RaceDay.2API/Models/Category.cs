using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay._2API.Models;

[Table("Categories")]
public class Category
{
    [Key]
    public int CategoryID { get; set; }

    public int EventID { get; set; }

    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [ForeignKey(nameof(EventID))]
    public Event? Event { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}