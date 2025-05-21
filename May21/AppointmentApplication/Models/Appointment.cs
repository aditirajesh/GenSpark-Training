using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentApplication.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int PatientAge { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; } = string.Empty;

        public void TakeArgumentsFromUser()
        {
            int id, age;
            DateTime date;
            Console.Write("Enter Appointment ID: ");
            while (!int.TryParse(Console.ReadLine(),out id)|| (id <= 0))
            {
                Console.WriteLine("Please enter positive integer.");
            }
            Id = id;

            Console.Write("Enter Patient Name: ");
            PatientName = Console.ReadLine();

            Console.Write("Enter Patient Age: ");
            while (!int.TryParse(Console.ReadLine(), out age) || (age <= 0))
            {
                Console.WriteLine("Please enter a valid age");
            }
            PatientAge = age;

            Console.Write("Enter Appointment Date DD/MM/YYYY: ");
            while (!DateTime.TryParse(Console.ReadLine(), out date))
            {
                Console.WriteLine("Please enter a valid date");
            }
            AppointmentDate = date;

            Console.Write("Enter reason for doctor visit: ");
            string reason = Console.ReadLine();
            if (reason == null)
            {
                reason = string.Empty;
            }
            Reason = reason;


        }

        public override string ToString()
        {
            return $@" Appointment ID {Id}
                    Patient name {PatientName}
                    Patient Age {PatientAge}
                    Appointment Date {AppointmentDate}
                    Reason {Reason}";
        }
        
    }
}
