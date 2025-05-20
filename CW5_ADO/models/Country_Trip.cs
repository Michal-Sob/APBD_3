namespace CW5_ADO.models.YourNamespace.Models;

public class Country_Trip
{
    public int IdCountry { get; set; }
    public int IdTrip { get; set; }
    public virtual Country Country { get; set; }
}