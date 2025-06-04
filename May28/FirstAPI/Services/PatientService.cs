using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;
using AutoMapper;
using FirstAPI.Misc;

namespace FirstAPI.Services
{
    public class PatientService : IPatientService
    {
        private readonly IRepository<int, Patient> _patientrepository;
        private readonly IRepository<string, User> _userrepository;
        private readonly IEncryptionService _encryptionservice;
        private readonly IMapper _mapper;
        private readonly PatientMapper _patientmapper;

        public PatientService(IRepository<string, User> userRepository,
                            IEncryptionService encryptionService,
                            IMapper mapper,
                            PatientMapper patientMapper,
                            IRepository<int, Patient> patientRepository)
        {
            _userrepository = userRepository;
            _encryptionservice = encryptionService;
            _mapper = mapper;
            _patientmapper = patientMapper;
            _patientrepository = patientRepository;

        }

            public async Task<Patient> AddPatient(PatientAddRequestDto dto)
            {
                try
                {
                    var user = _mapper.Map<PatientAddRequestDto, User>(dto);
                    var encryptedData = await _encryptionservice.EncryptData(new EncryptModel
                    {
                        Data = dto.Password //encrypting the password
                    });
                    user.Password = encryptedData.EncryptedData;
                    user.HashKey = encryptedData.HashKey;
                    user.Role = "Patient";
                    user = await _userrepository.Add(user);

                    var newPatient = _patientmapper.MapPatientAddRequestPatient(dto);
                    newPatient.User = user;
                    newPatient = await _patientrepository.Add(newPatient);
                    if (newPatient == null)
                    {
                        throw new Exception("Unable to create Patient");
                    }
                    return newPatient;

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                               
            }

        public async Task<Patient> GetPatientByName(string name)
        {
            var patients = await _patientrepository.GetAll();

            if (patients == null || !patients.Any())
            {
                throw new Exception("No patients found.");
            }

            var patient = patients.FirstOrDefault(p => p.Name.Trim().ToLower() == name.Trim().ToLower());

            if (patient == null)
            {
                throw new Exception("User not found");
            }

            return patient;
        }

    }

}