using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Repositories;
using FirstAPI.Contexts;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IRepository<int, Doctor> _doctorRepository;
        private readonly IRepository<int, Speciality> _specialityRepository;
        private readonly IRepository<int, DoctorSpeciality> _doctorSpecialityRepository;
        public DoctorService(IRepository<int, Doctor> doctorRepository,
                            IRepository<int, Speciality> specialityRepository,
                            IRepository<int, DoctorSpeciality> doctorSpecialityRepository)
        {
            _doctorRepository = doctorRepository;
            _specialityRepository = specialityRepository;
            _doctorSpecialityRepository = doctorSpecialityRepository;

        }

        public async Task<Doctor> GetDoctByName(string name)
        {
            var doctors = await _doctorRepository.GetAll();
            var doctor = doctors.FirstOrDefault(d => d.Name == name);
            if (doctor == null)
            {
                throw new Exception($"No doctor with name {name} found.");

            }
            return doctor;
        }

        public async Task<ICollection<Doctor>> GetDoctorsBySpeciality(string speciality)
        {
            var allSpecialities = await _specialityRepository.GetAll();
            var matchingSpeciality = allSpecialities.FirstOrDefault(s => s.Name.Equals(speciality));

            if (matchingSpeciality == null)
                throw new Exception("Speciality not found.");

            if (matchingSpeciality.DoctorSpecialities == null || matchingSpeciality.DoctorSpecialities.Count() == 0)
                throw new Exception("No doctors found under this speciality.");

            return matchingSpeciality.DoctorSpecialities
                                    .Select(ds => ds.Doctor)
                                    .ToList();
        }


        public async Task<Doctor> AddDoctor(DoctorAddRequestDto dto)
        {
            var doctor = new Doctor
            {
                Name = dto.Name,
                YearsOfExperience = dto.YearsOfExperience,
                DoctorSpecialities = new List<DoctorSpeciality>()
            };

            foreach (var add_speciality in dto.Specialities)
            {
                var specialities = await _specialityRepository.GetAll();
                var speciality = specialities.FirstOrDefault(s => s.Name == add_speciality.Name);
                if (speciality != null)
                {
                    DoctorSpeciality doctorspeciality = new DoctorSpeciality(doctor.Id, speciality.Id, doctor, speciality);

                    doctor.DoctorSpecialities.Add(doctorspeciality);
                }

            }

            return await _doctorRepository.Add(doctor);
        }
    }
}