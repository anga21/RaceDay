using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class RegisterDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    // "Organiser" or "Participant"
    [Required]
    public string Role { get; set; } = string.Empty;

    // Only needed when Role is "Organiser"
    public string? OrganiserCode { get; set; }
}