using System.ComponentModel.DataAnnotations.Schema;
using FirstAPI.Models;

namespace FirstAPI.Models
{
    public class Appointment
    {
        public int Id{ get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; } = string.Empty;

       
    }
}

