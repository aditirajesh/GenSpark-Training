using System.ComponentModel.DataAnnotations.Schema;
using FirstAPI.Models;

namespace FirstAPI.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string? Status { get; set; } = string.Empty;

        public Appointment()
        {

        }

        public Appointment(Patient patient, Doctor doctor, DateTime date)
        {
            this.Patient = patient;
            this.Doctor = doctor;
            AppointmentDate = date;
            PatientId = this.Patient.Id;
            DoctorId = this.Doctor.Id;
        }

        public static Appointment? TakeArgumentsFromUser(List<Patient> patients, List<Doctor> doctors)
        {
            Console.WriteLine("Please enter Patient ID");
            int patient_id;
            while (!int.TryParse(Console.ReadLine(), out patient_id) || patient_id <= 0)
            {
                Console.WriteLine("Invalid entry for ID. Please enter a valid patient ID");
            }

            var patient = patients.FirstOrDefault(p => p.Id == patient_id);
            if (patient == null)
            {
                Console.WriteLine("Patient not found. Enter valid patient ");
                return null;
            }

            Console.WriteLine("Please enter Doctor ID");
            int doctor_id;
            while (!int.TryParse(Console.ReadLine(), out doctor_id) || doctor_id <= 0)
            {
                Console.WriteLine("Invalid entry for ID. Please enter a valid doctor ID");
            }

            var doctor = doctors.FirstOrDefault(d => d.Id == doctor_id);
            if (doctor == null)
            {
                Console.WriteLine("Doctor not found.");
                return null;
            }

            Console.WriteLine("Enter Appointment Date (future date):");
            DateTime date;
            while (!DateTime.TryParse(Console.ReadLine(), out date) || date <= DateTime.Now)
            {
                Console.WriteLine("Invalid entry date, please enter a valid future date");
            }

            var appointment = new Appointment(patient, doctor, date);

            patient.Appointments.Add(appointment);
            doctor.Appointments.Add(appointment);

            return appointment;
        }
    }
}

