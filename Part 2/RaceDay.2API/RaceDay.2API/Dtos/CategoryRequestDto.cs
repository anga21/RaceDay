using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class CategoryRequestDto
{
    
    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }
}