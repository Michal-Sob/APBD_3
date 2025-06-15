using CW6.Data;
using CW6.Dtos;
using CW6.Models;
using Microsoft.EntityFrameworkCore;

namespace CW6.Services;

public class AppService : IAppService
{
    private const int MaxMedicamentsPerPrescription = 10;
    private readonly MyDbContext _context;

    public AppService(MyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ICollection<PatientGetDto>> GetAllPatientsAsync()
    {
        return await _context.Patients
            .Include(p => p.Prescription)
                .ThenInclude(pr => pr.Doctor)
            .Include(p => p.Prescription)
                .ThenInclude(pr => pr.PrescriptionMedicament)
                .ThenInclude(pm => pm.Medicament)
            .Select(patient => new PatientGetDto
            {
                IdPatient = patient.IdPatient,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                Prescriptions = patient.Prescription.Select(prescription => new PrescriptionGetDto
                {
                    IdPrescription = prescription.IdPrescription,
                    Date = prescription.Date,
                    DueDate = prescription.DueDate,
                    Doctor = new DoctorGetDto
                    {
                        IdDoctor = prescription.IdDoctor,
                        FirstName = prescription.Doctor.FirstName,
                        LastName = prescription.Doctor.LastName,
                        Email = prescription.Doctor.Email,
                    },
                    Medicaments = prescription.PrescriptionMedicament.Select(pm => new MedicamentGetDto
                    {
                        IdMedicament = pm.IdMedicament,
                        Dose = pm.Dose,
                        Details = pm.Details,
                        Description = pm.Medicament.Description,
                        Name = pm.Medicament.Name,
                        Type = pm.Medicament.Type,
                    }).ToList()
                }).ToList(),
            })
            .ToListAsync();
    }

    public async Task<PatientDto?> GetPatientAsync(int id)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPatient == id);

        return patient == null ? null : MapToPatientDto(patient);
    }

    public async Task<PrescriptionGetDto> CreatePrescriptionAsync(PrescriptionPostDto prescription)
    {
        if (prescription == null)
            throw new ArgumentNullException(nameof(prescription));

        await ValidatePrescriptionAsync(prescription);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var patient = await GetOrCreatePatientAsync(prescription.Patient);
            var newPrescription = await CreatePrescriptionEntityAsync(prescription, patient.IdPatient);
            
            await transaction.CommitAsync();
            
            return await MapToPrescriptionGetDtoAsync(newPrescription);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task ValidatePrescriptionAsync(PrescriptionPostDto prescription)
    {
        if (prescription.Medicaments?.Count > MaxMedicamentsPerPrescription)
        {
            throw new ArgumentException($"Prescription cannot contain more than {MaxMedicamentsPerPrescription} medicaments");
        }

        if (prescription.DueDate < prescription.Date)
        {
            throw new ArgumentException("Due date cannot be earlier than prescription date");
        }

        await ValidateMedicamentsExistAsync(prescription.Medicaments);
        await ValidateDoctorExistsAsync(prescription.IdDoctor);
    }

    private async Task ValidateMedicamentsExistAsync(ICollection<MedicamentPostDto>? medicaments)
    {
        if (medicaments == null || !medicaments.Any())
            return;

        var medicamentIds = medicaments.Select(m => m.IdMedicament).ToList();
        
        var existingIds = await _context.Medicaments
            .Where(m => medicamentIds.Contains(m.IdMedicament))
            .Select(m => m.IdMedicament)
            .ToListAsync();

        var missingIds = medicamentIds.Except(existingIds).ToList();

        if (missingIds.Any())
        {
            throw new ArgumentException($"Medicaments with IDs [{string.Join(", ", missingIds)}] do not exist");
        }
    }

    private async Task ValidateDoctorExistsAsync(int doctorId)
    {
        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.IdDoctor == doctorId);

        if (!doctorExists)
        {
            throw new ArgumentException($"Doctor with ID {doctorId} does not exist");
        }
    }

    private async Task<PatientDto> GetOrCreatePatientAsync(PatientPostDto patientDto)
    {
        var existingPatient = await GetPatientAsync(patientDto.IdPatient);
        
        return existingPatient ?? await CreatePatientAsync(patientDto);
    }

    private async Task<PatientDto> CreatePatientAsync(PatientPostDto patientDto)
    {
        var patient = new Patient
        {
            BirthDate = patientDto.DateOfBirth,
            FirstName = patientDto.FirstName,
            LastName = patientDto.LastName,
        };

        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();

        return MapToPatientDto(patient);
    }

    private async Task<Prescription> CreatePrescriptionEntityAsync(PrescriptionPostDto prescription, int patientId)
    {
        var prescriptionEntity = new Prescription
        {
            Date = prescription.Date,
            DueDate = prescription.DueDate,
            IdPatient = patientId,
            IdDoctor = prescription.IdDoctor,
            PrescriptionMedicament = prescription.Medicaments?.Select(m => new PrescriptionMedicament
            {
                IdMedicament = m.IdMedicament,
                Dose = m.Dose,
                Details = m.Details,
            }).ToList() ?? new List<PrescriptionMedicament>()
        };
        
        await _context.Prescriptions.AddAsync(prescriptionEntity);
        await _context.SaveChangesAsync();

        return prescriptionEntity;
    }

    private async Task<PrescriptionGetDto> MapToPrescriptionGetDtoAsync(Prescription prescription)
    {
        var prescriptionWithData = await _context.Prescriptions
            .Include(p => p.Doctor)
            .Include(p => p.PrescriptionMedicament)
                .ThenInclude(pm => pm.Medicament)
            .FirstAsync(p => p.IdPrescription == prescription.IdPrescription);

        return new PrescriptionGetDto
        {
            IdPrescription = prescriptionWithData.IdPrescription,
            Date = prescriptionWithData.Date,
            DueDate = prescriptionWithData.DueDate,
            Doctor = new DoctorGetDto
            {
                IdDoctor = prescriptionWithData.Doctor.IdDoctor,
                FirstName = prescriptionWithData.Doctor.FirstName,
                LastName = prescriptionWithData.Doctor.LastName,
                Email = prescriptionWithData.Doctor.Email,
            },
            Medicaments = prescriptionWithData.PrescriptionMedicament.Select(pm => new MedicamentGetDto
            {
                IdMedicament = pm.IdMedicament,
                Dose = pm.Dose,
                Details = pm.Details,
                Description = pm.Medicament.Description,
                Name = pm.Medicament.Name,
                Type = pm.Medicament.Type,
            }).ToList()
        };
    }

    private static PatientDto MapToPatientDto(Patient patient)
    {
        return new PatientDto
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            BirthDate = patient.BirthDate,
        };
    }
}