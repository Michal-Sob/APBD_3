namespace CW6.Dtos;

public class MedicamentGetDto
{
    public int IdMedicament { get; set; }
    public int? Dose { get; set; }
    public string Details { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
}