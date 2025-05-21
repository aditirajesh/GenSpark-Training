using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentApplication.Interfaces;
using AppointmentApplication.Exceptions;

namespace AppointmentApplication.Respositories
{
    public abstract class Repository<K,T>:IRepository<K,T> where T:class
    {
        protected List<T> items = new List<T>();
        public abstract K GenerateId();

        public abstract T GetById(K id);
        public abstract ICollection<T> GetAll();

        public T Add(T item)
        {
            var id = GenerateId();
            var property = typeof(T).GetProperty("Id");

            if (property != null)
            {
                property.SetValue(item, id);
            }

            if (items.Contains(item))
            {
                throw new DuplicateEntityException("Appointment already exists.");
            }
            items.Add(item);
            return item;
        }
       
    }
}
