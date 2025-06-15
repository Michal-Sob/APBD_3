namespace CW6.Dtos;

public class PrescriptionGetDto
{
    public int IdPrescription { get; set; }
    public DateOnly Date { get; set; }
    public DateOnly DueDate { get; set; }
    public ICollection<MedicamentGetDto> Medicaments { get; set; }
    public DoctorGetDto Doctor { get; set; }
}