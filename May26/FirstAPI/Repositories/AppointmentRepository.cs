using FirstAPI.Models;
namespace FirstAPI.Repositories
{
    public class AppointmentRepository : Repository<int, Appointment>
    {
        public AppointmentRepository() : base()
        {

        }

        public override ICollection<Appointment> GetAll()
        {
            if (_items.Count == 0)
            {
                throw new Exception("Collection is empty");
            }
            return _items;
        }

        public override Appointment GetByID(int id)
        {
            Appointment appointment = _items.FirstOrDefault(x => x.Id == id);
            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }
            return appointment;
        }

        public override int GenerateID()
        {
            if (_items.Count == 0)
            {
                return 101;
            }
            else
            {
                return _items.Max(x => x.Id) + 1;
            }
        }
    }
    
}
