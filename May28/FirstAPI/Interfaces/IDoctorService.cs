using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Interfaces
{
    public interface IDoctorService
    {
        public Task<Doctor> GetDoctByName(string name);
        public Task<ICollection<DoctorsBySpecialityResponseDto>> GetDoctorsBySpeciality(string speciality);
        public Task<Doctor> AddDoctor(DoctorAddRequestDto doctor);
        Task<ICollection<Appointment>> ViewAppointments(int doctor_id);
        Task<int[]> MapAndAddSpeciality(DoctorAddRequestDto dto);
        
    }
}