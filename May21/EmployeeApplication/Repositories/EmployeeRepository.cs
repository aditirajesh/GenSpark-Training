using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeApplication.Models;
using EmployeeApplication.Exceptions;

namespace EmployeeApplication.Repositories
{
    public class EmployeeRepository: Repository<int,Employee>
    {
        public EmployeeRepository(): base() 
        {
        }

        public override ICollection<Employee> GetAll()
        {
            if (_items.Count == 0)
            {
                throw new CollectionEmptyException("No employees found");
            }
            return _items;
        }

        public override Employee GetById(int id)
        {
            var employee = _items.FirstOrDefault(x => x.Id == id);
            if (employee == null)
            {
                throw new KeyNotFoundException("No employee with that id");
            }
            return employee;
        }

        public override int GenerateId()
        {
            if (_items.Count == 0)
            {
                return 101;
            } else
            {
                return _items.Max(e => e.Id) + 1;
            }
        }
    }
}
