
using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class UpdateProfileDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}