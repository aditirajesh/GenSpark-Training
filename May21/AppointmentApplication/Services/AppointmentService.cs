using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentApplication.Interfaces;
using AppointmentApplication.Models;
using AppointmentApplication.Respositories;



namespace AppointmentApplication.Services
{
    public class AppointmentService : IAppointmentService
    {
        private IRepository<int, Appointment> _repository;

        public AppointmentService(IRepository<int,Appointment> repo)
        {
            _repository = repo;
        }

        public int Add(Appointment appointment)
        {
            try
            {
                var app = _repository.Add(appointment);
                if (app != null)
                {
                    return app.Id;
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return -1;
        }

        public List<Appointment> Search(AppointmentSearchModel searchModel)
        {
            try
            {
                var appointments = _repository.GetAll();
                appointments = SearchByName(appointments, searchModel.PatientName);
                appointments = SearchByDate(appointments, searchModel.AppointmentDate);
                appointments = SearchByAge(appointments, searchModel.AgeRange);
                if (appointments != null && appointments.Count > 0)
                {
                    return appointments.ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);  
            }
            return null;
        }

        public ICollection<Appointment> SearchByName(ICollection<Appointment> appointments, string? name)
        {
            if (name == null|| appointments == null|| appointments.Count == 0)
            {
                return appointments;
            } else
            {
                return appointments.Where(n => n.PatientName.ToLower().Contains(name.ToLower())).ToList();

            }
        }

        public ICollection<Appointment> SearchByDate(ICollection<Appointment> appointments, DateTime? date)
        {
            if (date == null || appointments == null || appointments.Count == 0)
            {
                return appointments;
            }
            return appointments.Where(n => n.AppointmentDate == date).ToList();
        }

        public ICollection<Appointment> SearchByAge(ICollection<Appointment> appointments, Range<int>? age)
        {
            if (age == null || appointments == null || appointments.Count == 0)
            {
                return appointments;
            }
            return appointments.Where(n => n.PatientAge >= age.Min_Val && n.PatientAge <= age.Max_Val).ToList();
        }
    }
}
