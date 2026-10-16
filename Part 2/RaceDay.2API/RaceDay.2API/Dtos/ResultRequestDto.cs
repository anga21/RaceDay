using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class ResultRequestDto
{
   
    public TimeOnly? FinishTime { get; set; }

    [Range(1, 100000)]
    public int? Position { get; set; }

    [MaxLength(10)]
    public string? BibNumber { get; set; }
}