using System.ComponentModel.DataAnnotations;

namespace CW6.Dtos;

public class PatientPostDto
{
    [Required]
    public int IdPatient { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;
    
    [Required]
    public DateOnly DateOfBirth { get; set; }
}