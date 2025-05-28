using FirstAPI.Models;
namespace FirstAPI.Interfaces
{
    public interface IAppointmentService
    {
        int AddAppointment();
        Appointment DeleteAppointment(int id);

        void GetAppointment(int id);

        void GetAllAppointments();
    }
}
