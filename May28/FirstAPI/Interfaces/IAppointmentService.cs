using FirstAPI.Models;
using FirstAPI.Models.DTOs;
namespace FirstAPI.Interfaces
{
    public interface IAppointmentService
    {
        public Task<Appointment> MakeAppointment(AppointmentAddRequestDto dto);
        public Task<Appointment> CancelAppointment(int appointment_id);
    }
}
