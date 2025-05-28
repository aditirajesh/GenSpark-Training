using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FirstAPI.Repositories
{
    public abstract class PatientRepository : Repository<int, Patient>
    {
        protected PatientRepository(ClinicContext clinicContext) : base(clinicContext) { }
        public override async Task<Patient> GetByID(int key)
        {
            var patient = await _clinicContext.Patients.SingleOrDefaultAsync(p => p.Id == key);
            return patient??throw new Exception("No patient with the given ID");
        }

        public override async Task<IEnumerable<Patient>> GetAll()
        {
            var patients = _clinicContext.Patients;
            if (patients.Count() == 0)
                throw new Exception("No Patients in the database");
            return await patients.ToListAsync();
        }
    }
}