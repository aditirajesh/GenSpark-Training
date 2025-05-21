using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentApplication.Respositories;
using AppointmentApplication.Models;
using AppointmentApplication.Exceptions;
using AppointmentApplication.Interfaces;


namespace AppointmentApplication.Respositories
{
    public class AppointmentRepository : Repository<int,Appointment>
    {
        public AppointmentRepository(): base()
        {
        }

        public  override int GenerateId()
        {
            if (items.Count == 0)
            {
                return 101;
            } else
            {
                return items.Max(i => i.Id) + 1;
            }

        }

        public override Appointment GetById(int id)
        {
            var employee = items.FirstOrDefault(i => i.Id == id);
            if (employee == null)
            { 
                throw new KeyNotFoundException();
            }
            return employee;
        }

        public override ICollection<Appointment> GetAll()
        {
            if (items.Count == 0)
            {
                throw new CollectionEmptyException("No Appointments");
            }
            return items;
        }
    }
}
