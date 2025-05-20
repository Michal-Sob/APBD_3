namespace CW5_ADO.models.DTO;

public class ClientTripDTO
{
    public int IdClient { get; set; }
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
    public List<CountrySummaryDTO> Countries { get; set; }
}
