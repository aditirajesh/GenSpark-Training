using FirstAPI.Contexts;
using FirstAPI.Interfaces;
using FirstAPI.Misc;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<int, Doctor> _doctorRepository;
        private readonly IRepository<int, Appointment> _appointmentRepository;
        private readonly IRepository<int, Patient> _patientRepository;
        private readonly AppointmentMapper _appointmentMapper;
        private readonly ClinicContext _clinicContext;

        public AppointmentService(IRepository<int, Appointment> appointmentRepository,
                                    IRepository<int, Doctor> doctorRepository,
                                    IRepository<int, Patient> patientRepository,
                                    AppointmentMapper appointmentMapper,
                                    ClinicContext clinicContext)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _appointmentMapper = appointmentMapper;
            _clinicContext = clinicContext;                        
        }

        public async Task<Appointment> CancelAppointment(int appointment_id)
        {
            var cancel_appointment = await _appointmentRepository.GetByID(appointment_id);
            if (cancel_appointment == null)
            {
                throw new Exception("Appointment Not Found");
            }
            if (cancel_appointment.Status == "Completed")
            {
                throw new Exception("Cannot cancel past appointments");
            }

            cancel_appointment.Status = "Cancelled";
            await _clinicContext.SaveChangesAsync();
            return cancel_appointment;

        }

        public async Task<Appointment> MakeAppointment(AppointmentAddRequestDto dto)
        {
            var appointment = _appointmentMapper.MapRequestToAppointment(dto);
            var doctor = await _doctorRepository.GetByID(appointment.DoctorId);
            var patient = await _patientRepository.GetByID(appointment.PatientId);
            if (doctor == null)
            {
                throw new Exception("Doctor not found");
            }
            if (patient == null)
            {
                throw new Exception("Patient not found");
            }

            if (doctor.Appointments == null)
            {
                doctor.Appointments = new List<Appointment>();
            }

            if (patient.Appointments == null)
            {
                patient.Appointments = new List<Appointment>();
            }

            doctor.Appointments.Add(appointment);
            appointment.Doctor = doctor;
            appointment.Patient = patient;

            if (appointment.AppointmentDate > DateTime.Now)
            {
                appointment.Status = "Upcoming";
            }
            else
            {
                appointment.Status = "Completed";
            }
            appointment = await _appointmentRepository.Add(appointment);
            if (appointment == null)
            {
                throw new Exception("Appointment could not be created");
            }
            return appointment;
        }


    }
}