using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeApplication.Models;
using EmployeeApplication.Interfaces;

namespace EmployeeApplication.Services
{
    public class EmployeeService: IEmployeeService
    {
        IRepository<int, Employee> _employeerepository;

        public EmployeeService(IRepository<int,Employee> emprepository)
        {
            _employeerepository = emprepository;
        }

        public int AddEmployee(Employee emp)
        {
            try
            {
                var result = _employeerepository.Add(emp);
                if (result != null)
                {
                    return result.Id;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return -1;
        }

        public List<Employee>? SearchEmployee(SearchModel searchModel)
        {
            try
            {
                var employees = _employeerepository.GetAll();
                employees = SearchById(employees, searchModel.Id);
                employees = SearchByName(employees, searchModel.Name);
                employees = SearchByAge(employees, searchModel.Age);
                employees = SearchBySalary(employees, searchModel.Salary);

                if (employees != null && employees.Count > 0)
                    return employees.ToList(); ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        public ICollection<Employee> SearchById(ICollection<Employee> employees, int? id)
        {
           
            if (id==null ||employees.Count ==0 || employees == null)
            {
                return employees;
            } else
            {
                return employees.Where(n => n.Id == id).ToList();
            }
            
        }

        public ICollection<Employee> SearchByName(ICollection<Employee> employees, string? name)
        {

            if (name == null || employees.Count == 0 || employees == null)
            {
                return employees;
            }
            else
            {
                return employees.Where(e => e.Name.ToLower().Contains(name.ToLower())).ToList();
            }

        }

        public ICollection<Employee> SearchBySalary(ICollection<Employee> employees, Range<double>? salary)
        {

            if (salary == null || employees.Count == 0 || employees == null)
            {
                return employees;
            }
            else
            {
                return employees.Where(n => n.Salary >= salary.MinVal && n.Salary <= salary.MaxVal).ToList();
            }

        }

        public ICollection<Employee> SearchByAge(ICollection<Employee> employees, Range<int>? age)
        {

            if (age == null || employees.Count == 0 || employees == null)
            {
                return employees;
            }
            else
            {
                return employees.Where(n => n.Age >= age.MinVal && n.Age <= age.MaxVal).ToList();
            }

        }


    }
}
