using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstAPI.Models
{
    public class DoctorSpeciality
    {
        public int SerialNumber { get; set; }
        public int DoctorId { get; set; }
        public int SpecialityId { get; set; }
        public Speciality? Speciality { get; set; }
        public Doctor? Doctor { get; set; }

        public DoctorSpeciality()
        {
        }

        public DoctorSpeciality(int doctorid, int specialityid, Doctor? doctor = null, Speciality? speciality = null)
        {
            DoctorId = doctorid;
            SpecialityId = specialityid;
            Doctor = doctor;
            Speciality = speciality;
        }
    }

    
}