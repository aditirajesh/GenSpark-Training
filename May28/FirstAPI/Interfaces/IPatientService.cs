using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Interfaces
{
    public interface IPatientService
    {
        Task<Patient> AddPatient(PatientAddRequestDto dto);
        Task<Patient> GetPatientByName(string name);
    }
}