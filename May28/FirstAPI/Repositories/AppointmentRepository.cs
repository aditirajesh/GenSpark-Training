using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FirstAPI.Repositories
{
    public  class AppointmentRepository : Repository<int, Appointment>
    {
        public AppointmentRepository(ClinicContext clinicContext) : base(clinicContext) { }

        public override async Task<Appointment> GetByID(int id)
        {
            var item = await _clinicContext.Appointments.SingleOrDefaultAsync(a => a.Id == id);
            if (item != null)
            {
                return item;
            }
            throw new Exception("item not found");

        }

        public override async Task<IEnumerable<Appointment>> GetAll()
        {
            var appointments = _clinicContext.Appointments;
            if (appointments.Count() == 0)
            {
                throw new Exception("No Appointments in the database");
            }
            return await appointments.ToListAsync();
        }
    }
}