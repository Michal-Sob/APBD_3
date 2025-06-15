using CW6.Models;
using Microsoft.EntityFrameworkCore;

namespace CW6.Data;

public class MyDbContext : DbContext
{
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
        base.Database.EnsureCreated();

        if (!Doctors.Any())
        {
            Doctors.Add(new Doctor
            {
                FirstName = "Antoni",
                LastName = "Kwiatkowski",
                Email = "antoni.kwiatkowski@example.com"
            });
        }

        if (!Medicaments.Any())
        {
            Medicaments.Add(new Medicament
            {
                Name = "Paracetamol",
                Description = "Pain reliever and fever reducer",
                Type = "Tablet"
            });
        }
        
        SaveChanges();
    }
}