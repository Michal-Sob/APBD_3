using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CW6.Models;

[Table("Patient")]
public class Patient
{
    
    [Key]
    public int IdPatient { get; set; }
    
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;
    
    [MaxLength(100)]
    public string LastName { get; set; } = null!;
    
    public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    public virtual List<Prescription> Prescription { get; set; } = null!;
}