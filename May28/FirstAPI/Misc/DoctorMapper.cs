using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Misc
{
    public class DoctorMapper
    {
        public virtual Doctor? MapDoctorAddRequestDoctor(DoctorAddRequestDto addRequestDto)
        {
            Doctor doctor = new();
            doctor.Name = addRequestDto.Name;
            doctor.YearsOfExperience = addRequestDto.YearsOfExperience;
            doctor.Email = addRequestDto.Email;
            return doctor;
        }
    }
}