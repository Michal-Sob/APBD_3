using CW6.Dtos;

namespace CW6.Services;

public interface IAppService
{
    Task<ICollection<PatientGetDto>> GetAllPatientsAsync();
    Task<PrescriptionGetDto> CreatePrescriptionAsync(PrescriptionPostDto prescription);
    Task<PatientDto?> GetPatientAsync(int id);
}