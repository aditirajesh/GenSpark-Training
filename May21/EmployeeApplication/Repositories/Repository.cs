using EmployeeApplication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeApplication.Exceptions;

namespace EmployeeApplication.Repositories
{
    public abstract class Repository<K,T>:IRepository<K,T> where T: class
    {
        protected List<T> _items = new List<T>();
        public abstract K GenerateId();
        public abstract T GetById(K id);
        public abstract ICollection<T> GetAll();

        public T Add(T item)
        {
            var id = GenerateId();
            var property = typeof(T).GetProperty("Id");
            if (property != null)
            {
                property.SetValue(this, item);
            }

            if (_items.Contains(item))
            {
                throw new DuplicateEntityException("Employee already exists");
            }
            _items.Add(item);
            return item;
        }

        public T Delete(K id)
        {
            var item = GetById(id);
            if (item == null)
            {
                throw new KeyNotFoundException("Item not found");
            }
            _items.Remove(item);
            return item;
        }

        public T Update(T item)
        {
            var myItem = GetById((K)item.GetType().GetProperty("Id").GetValue(item));
            if (myItem == null)
            {
                throw new KeyNotFoundException("Item not found");
            }
            var index = _items.IndexOf(myItem);
            _items[index] = item;
            return item;
        }
    }
}
