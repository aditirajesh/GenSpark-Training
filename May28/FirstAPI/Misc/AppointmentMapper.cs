using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Misc
{
    public class AppointmentMapper
    {
        public Appointment? MapRequestToAppointment(AppointmentAddRequestDto dto)
        {
            var appointment = new Appointment();
            appointment.PatientId = dto.PatientId;
            appointment.DoctorId = dto.DoctorId;
            appointment.AppointmentDate = dto.AppointmentDate;
            return appointment;
        }
    }
}