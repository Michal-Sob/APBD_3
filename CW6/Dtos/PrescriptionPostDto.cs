using System.ComponentModel.DataAnnotations;

namespace CW6.Dtos;

public class PrescriptionPostDto
{
    [Required]
    public DateOnly Date { get; set; }
    
    [Required]
    public DateOnly DueDate { get; set; }
    
    [Required]
    public PatientPostDto Patient { get; set; }
    
    [Required]
    public ICollection<MedicamentPostDto> Medicaments { get; set; }
    
    public int IdDoctor { get; set; }
}