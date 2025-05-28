using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FirstAPI.Repositories
{
    public abstract class DoctorRepository : Repository<int, Doctor>
    {
        public DoctorRepository(ClinicContext clinicContext) : base(clinicContext) { }

        public override async Task<Doctor> GetByID(int id)
        {
            var item = await _clinicContext.Doctors.SingleOrDefaultAsync(a => a.Id == id);
            if (item != null)
            {
                return item;
            }
            throw new Exception("item not found");

        }

        public override async Task<IEnumerable<Doctor>> GetAll()
        {
            var doctors = _clinicContext.Doctors;
            if (doctors.Count() == 0)
            {
                throw new Exception("No doctors in the database");
            }
            return await doctors.ToListAsync();
        }
    }
}