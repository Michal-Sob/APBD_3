using System.ComponentModel.DataAnnotations;

namespace CW6.Dtos;

public class MedicamentPostDto
{
    [Required]
    public int IdMedicament { get; set; }
    public int? Dose { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Details { get; set; } = null!;
}