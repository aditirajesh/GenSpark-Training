using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentApplication.Services;
using AppointmentApplication.Models;
using AppointmentApplication.Interfaces;
using AppointmentApplication.Respositories;

namespace AppointmentApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IRepository<int, Appointment> appointmentrepo = new AppointmentRepository();
            IAppointmentService appointmentService = new AppointmentService(appointmentrepo);
            ManageAppointments manageappointments = new ManageAppointments(appointmentService);
            manageappointments.StartApplication();

        }
    }
}