using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentApplication.Models;

namespace AppointmentApplication.Interfaces
{
    public interface IAppointmentService
    {
        int Add(Appointment appointment);
        List<Appointment> Search(AppointmentSearchModel searchModel);
        ICollection<Appointment> SearchByName(ICollection<Appointment> appointments, string? name);
        ICollection<Appointment> SearchByDate(ICollection<Appointment> appointments, DateTime? date);
        ICollection<Appointment> SearchByAge(ICollection<Appointment> appointments, Range<int>? age);

    }
}
