using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeApplication.Models;

namespace EmployeeApplication
{
    internal class Program
    {
        public delegate void MyDelegate(int n1, int n2);

        List<Employee> employees = new List<Employee>
            {
                new Employee(101,30, "John Doe",  50000),
                new Employee(102, 25,"Jane Smith",  60000),
                new Employee(103,35, "Sam Brown",  70000)
            };

        public void Add(int n1, int n2)
        {
            int sum = n1 + n2;
            Console.WriteLine($"Sum of {n1} and {n2} is: {sum}");
        }

        public void Product(int n1, int n2)
        {
            int product = n1 + n2;
            Console.WriteLine($"Product of {n1} and {n2} is: {product}");
        }

        public void FindEmployees()
        {
            int empid = 102;
            Predicate<Employee> predicate = e => e.Id == empid;
            Employee? emp = employees.Find(predicate);
            Console.WriteLine(emp.ToString() ?? "No Employee found");
        }

        public void SortEmployees()
        {
            var sortedEmployees = employees.OrderBy(e => e.Name);
            foreach (Employee emp in sortedEmployees)
            {
                Console.WriteLine(emp);
            }
        }

        Program()
        {
            //MyDelegate operation = new MyDelegate(Add); - using defined delegate
            Action<int, int> operation = Add; //predefined delegate 
            operation += Product;
            operation += (int n1, int n2) => Console.WriteLine($"Division of {n1} and {n2} is: ", n1 / n2); //lambda function 
            operation += delegate (int n1, int n2)
            {
                int difference = n1 - n2;
                Console.WriteLine($"Difference of {n1} and {n2} is: {difference}");
            }; //anonymous method 

            operation(10, 20);
        }
        static void Main(string[] args)
        {
            //IRepository<int, Employee> employeeRepository = new EmployeeRepository(); - dependency inversion, loose coupling
            //- associating with the functionality, not the object itself 
            //IEmployeeService employeeService = new EmployeeService(employeeRepository);
            //ManageEmployee manageEmployee = new ManageEmployees(employeeService);
            //manageEmployee.Start();


            //new Program();
            //Program program = new();
            //program.FindEmployees();
            //program.SortEmployees();
            //FindEmployees();

        }
    }
}