using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentApplication.Services;
using AppointmentApplication.Models;
using AppointmentApplication.Interfaces;


namespace AppointmentApplication
{
    public class ManageAppointments
    {
        private readonly IAppointmentService _appointmentService;
        public ManageAppointments(IAppointmentService appService) {
            _appointmentService = appService;
        }

        public void StartApplication()
        {
            bool condition = true;
            while (condition) {
                int choice = 0;
                Console.WriteLine("This is an Patient Appointment Management System.");
                Console.WriteLine(@"1: Add Appointment
                2: Search for Appointment
                3:Quit");


                while (!int.TryParse(Console.ReadLine(), out choice) || !(choice >= 1 && choice <= 3))
                {
                    Console.Write("Invalid Input. Please enter 1 or 2: ");
                }

                switch(choice)
                {
                    case 1:
                        AddAppointment();
                        condition = true;
                        break;

                    case 2:
                        SearchAppointment();
                        condition = true;
                        break;

                    case 3:
                        Console.WriteLine("Goodbye!");
                        condition = false;
                        break;
                }

            }
            

        }

        public void AddAppointment()
        {
            Appointment appointment = new Appointment();
            appointment.TakeArgumentsFromUser();
            int id = _appointmentService.Add(appointment);
            if (id != -1)
            {
                Console.WriteLine($"Addition successful. The ID is: {id}");
            } else
            {
                Console.WriteLine($"Additio unsuccessful. Please check and try again.");
            }
        }

        public void SearchAppointment()
        {
            AppointmentSearchModel user_options = SearchOptions();
            Console.WriteLine($"user chose options: ", user_options);
            var appointments = _appointmentService.Search(user_options);
            if (appointments == null)
            {
                Console.WriteLine("No appointments found.");
            } else
            {
                Console.WriteLine("Search Results: ");
                GetAppointments(appointments);
            }
        }

        public void GetAppointments(ICollection<Appointment> appointments)
        {
            foreach (Appointment appointment in appointments)
            {
                Console.WriteLine(appointment);
                Console.WriteLine();
            }
        }

        public AppointmentSearchModel SearchOptions()
        {
            int option = 0;
            AppointmentSearchModel searchModel = new AppointmentSearchModel();
            Console.Write("Search by Name, 1:Yes, 2:No : ");
            while (!int.TryParse(Console.ReadLine(), out option) || !(option >= 1 && option <= 2))
            {
                Console.Write("Please enter 1 or 2 : ");
            }
            if (option == 1)
            {
                Console.Write("Enter Name: ");
                searchModel.PatientName = Console.ReadLine();
            } else
            {
                searchModel.PatientName = null;
            }
            option = 0;
            Console.WriteLine();

            Console.Write("Search by Date, 1:Yes, 2:No : ");
            while (!int.TryParse(Console.ReadLine(), out option) || !(option >= 1 && option <= 2))
            {
                Console.Write("Please enter 1 or 2 : ");
            }

            if (option == 1)
            {
                DateTime date;
                Console.Write("Enter date: ");
                while (!DateTime.TryParse(Console.ReadLine(),out date))
                {
                    Console.Write("Please enter a valid date: ");
                }
                searchModel.AppointmentDate = date;
            } else
            {
                searchModel.AppointmentDate= null;
            }
            option = 0;
            Console.WriteLine();

            Console.Write("Search by Age? 1:Yes, 2:No : ");
            while (!int.TryParse(Console.ReadLine(), out option) || !(option >= 1 && option <= 2))
            {
                Console.Write("Please enter 1 or 2 : ");
            }

            if (option == 1)
            {
                searchModel.AgeRange = new Range<int>();
                int min_age, max_age;
                Console.Write("Please enter minimum age: ");
                while (!int.TryParse(Console.ReadLine(),out min_age)|| min_age <= 0)
                {
                    Console.Write("Must be a valid age. Please try again: ");
                }

                Console.Write("Please enter maximum age: ");
                while (!int.TryParse(Console.ReadLine(), out max_age) || max_age <= 0)
                {
                    Console.Write("Must be a valid age. Please try again: ");
                }

                searchModel.AgeRange.Min_Val = min_age;
                searchModel.AgeRange.Max_Val = max_age;

            }

            return searchModel;



        }
    }
}
