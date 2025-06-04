using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FirstAPI.Repositories
{
    public  class SpecialityRepository : Repository<int, Speciality>
    {
        public SpecialityRepository(ClinicContext clinicContext) : base(clinicContext) { }

        public override async Task<Speciality> GetByID(int id)
        {
            var item = await _clinicContext.Specialities.SingleOrDefaultAsync(a => a.Id == id);
            if (item != null)
            {
                return item;
            }
            throw new Exception("item not found");

        }

        public override async Task<IEnumerable<Speciality>> GetAll()
        {
            var Specialities = _clinicContext.Specialities;
            if (Specialities.Count() == 0)
            {
                throw new Exception("No Specialities in the database");
            }
            return await Specialities.ToListAsync();
        }
    }
}