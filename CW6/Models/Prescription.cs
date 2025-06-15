using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CW6.Models;

[Table("Prescription")]
public class Prescription
{
    [Key]
    public int IdPrescription { get; set; }
    
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
    
    [Column("IdPatient")]
    public int IdPatient { get; set; }
    
    [Column("IdDoctor")]
    public int IdDoctor { get; set; }
    
    [ForeignKey(nameof(IdDoctor))]
    public virtual Doctor Doctor { get; set; } = null!;
    
    [ForeignKey(nameof(IdPatient))]
    public virtual Patient Patient { get; set; } = null!;
    
    public virtual List<PrescriptionMedicament> PrescriptionMedicament { get; set; } = null!;
}