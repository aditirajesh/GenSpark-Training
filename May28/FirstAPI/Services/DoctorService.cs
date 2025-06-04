using System.Threading.Tasks;
using FirstAPI.Interfaces;
using FirstAPI.Misc;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;
using Microsoft.VisualBasic;
using AutoMapper;


namespace FirstAPI.Services
{
    public class DoctorService : IDoctorService
    {
        DoctorMapper doctorMapper;
        SpecialityMapper specialityMapper;
        private readonly IRepository<int, Doctor> _doctorRepository;
        private readonly IRepository<int, Speciality> _specialityRepository;
        private readonly IRepository<int, DoctorSpeciality> _doctorSpecialityRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IOtherContextFunctionalities _otherContextFunctionalities;
        private readonly IEncryptionService _encryptionService;
        private readonly IMapper _mapper;
        private readonly IRepository<int, Appointment> _appointmentRepository;

        public DoctorService(IRepository<int, Doctor> doctorRepository,
                            IRepository<int, Speciality> specialityRepository,
                            IRepository<int, DoctorSpeciality> doctorSpecialityRepository,
                            IOtherContextFunctionalities otherContextFunctionities,
                            IRepository<string, User> userRepository,
                            IEncryptionService encryptionService,
                            IMapper mapper,
                            IRepository<int, Appointment> appointmentRepository)
        {
            doctorMapper = new DoctorMapper();
            specialityMapper = new SpecialityMapper();
            _doctorRepository = doctorRepository;
            _specialityRepository = specialityRepository;
            _doctorSpecialityRepository = doctorSpecialityRepository;
            _userRepository = userRepository;
            _otherContextFunctionalities = otherContextFunctionities;
            _encryptionService = encryptionService;
            _mapper = mapper;
            _appointmentRepository = appointmentRepository;

        }

        public async Task<Doctor> AddDoctor(DoctorAddRequestDto doctor)
        {
            try
            {
                var user = _mapper.Map<DoctorAddRequestDto, User>(doctor);
                var encryptedData = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = doctor.Password
                });
                user.Password = encryptedData.EncryptedData;
                user.HashKey = encryptedData.HashKey;
                user.Role = "Doctor";
                var newDoctor = doctorMapper.MapDoctorAddRequestDoctor(doctor);

                user.Doctor = newDoctor;
                newDoctor.User = user;
                user = await _userRepository.Add(user);

                if (newDoctor == null)
                    throw new Exception("Could not add doctor");
                if (doctor.Specialities.Count() > 0)
                {
                    int[] specialities = await MapAndAddSpeciality(doctor);
                    for (int i = 0; i < specialities.Length; i++)
                    {
                        var doctorSpeciality = specialityMapper.MapDoctorSpeciality(newDoctor.Id, specialities[i]);
                        doctorSpeciality = await _doctorSpecialityRepository.Add(doctorSpeciality);
                    }
                }

                return newDoctor;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<int[]> MapAndAddSpeciality(DoctorAddRequestDto doctor)
        {
            int[] specialityIds = new int[doctor.Specialities.Count()];
            IEnumerable<Speciality> existingSpecialities = null;
            try
            {
                existingSpecialities = await _specialityRepository.GetAll();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            int count = 0;
            foreach (var item in doctor.Specialities)
            {
                Speciality speciality = null;
                if (existingSpecialities != null)
                    speciality = existingSpecialities.FirstOrDefault(s => s.Name.ToLower() == item.Name.ToLower());
                if (speciality == null)
                {
                    speciality = specialityMapper.MapSpecialityAddRequestDoctor(item);
                    speciality = await _specialityRepository.Add(speciality);
                }
                specialityIds[count] = speciality.Id;
                count++;
            }
            return specialityIds;
        }

        public Task<Doctor> GetDoctByName(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<DoctorsBySpecialityResponseDto>> GetDoctorsBySpeciality(string speciality)
        {
            var result = await _otherContextFunctionalities.GetDoctorsBySpeciality(speciality);
            return result;
        }

        public async Task<ICollection<Appointment>> ViewAppointments(int doctor_id)
        {
            var doctor = await _doctorRepository.GetByID(doctor_id);
            if (doctor == null)
            {
                throw new Exception("Doctor not found");
            }
            if (doctor.Appointments == null || !doctor.Appointments.Any())
            {
                throw new Exception("No appointments found");
            }
            return doctor.Appointments.ToList();
        }
    }
    

}