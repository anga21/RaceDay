
using System.ComponentModel.DataAnnotations;

namespace RaceDay._2API.Dtos;

public class EnrolmentRequestDto
{
    
    [Range(1, int.MaxValue)]
    public int CategoryID { get; set; }
}