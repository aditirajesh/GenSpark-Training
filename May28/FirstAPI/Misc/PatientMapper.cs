using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Misc
{
    public class PatientMapper
    {
        public Patient MapPatientAddRequestPatient(PatientAddRequestDto dto)
        {
            Patient patient = new()
            {
                Name = dto.Name,
                Age = dto.Age,
                Email = dto.Email,
                Phone = dto.Phone
            };
            return patient;
        }
    }
}