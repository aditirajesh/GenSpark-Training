using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Misc
{
    public class SpecialityMapper
    {
        public virtual Speciality? MapSpecialityAddRequestDoctor(SpecialityAddRequestDto addRequestDto)
        {
            Speciality speciality = new();
            speciality.Name = addRequestDto.Name;
            return speciality;
        }

        public virtual DoctorSpeciality MapDoctorSpeciality(int doctorId, int specialityId)
        {
            DoctorSpeciality doctorSpeciality = new();
            doctorSpeciality.DoctorId = doctorId;
            doctorSpeciality.SpecialityId = specialityId;
            return doctorSpeciality;
        }
    }
}