using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FirstAPI.Repositories
{
    public abstract class DoctorSpecialityRepository : Repository<int, DoctorSpeciality>
    {
        public DoctorSpecialityRepository(ClinicContext clinicContext) : base(clinicContext) { }

        public override async Task<DoctorSpeciality> GetByID(int id)
        {
            var item = await _clinicContext.DoctorSpecialities.SingleOrDefaultAsync(a => a.Id == id);
            if (item != null)
            {
                return item;
            }
            throw new Exception("item not found");

        }

        public override async Task<IEnumerable<DoctorSpeciality>> GetAll()
        {
            var DoctorSpecialities = _clinicContext.DoctorSpecialities;
            if (DoctorSpecialities.Count() == 0)
            {
                throw new Exception("No Doctor Specialities in the database");
            }
            return await DoctorSpecialities.ToListAsync();
        }
    }
}