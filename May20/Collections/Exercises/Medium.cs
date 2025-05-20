using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BasicsofCollections;

namespace BasicsofCollections.Exercises
{
    internal class Medium
    {
        public static void Run()
        {
            Employee emp1 = new Employee(101, 28, "Ramu", 45000);
            Employee emp2 = new Employee(102, 32, "Ramu", 52000);
            Employee emp3 = new Employee(103, 26, "Somu", 48000);
            Employee emp4 = new Employee(104, 35, "Gomu", 60000);
            Employee emp5 = new Employee(105, 29, "Vimu", 50000);

            EmployeeDetails EmpDetails = new EmployeeDetails();
            EmpDetails.AddEmployee(emp1);
            EmpDetails.AddEmployee(emp2);
            EmpDetails.AddEmployee(emp3);
            EmpDetails.AddEmployee(emp4);
            EmpDetails.AddEmployee(emp5);


            //Employee emp_101 = EmpDetails.GetEmp(101);
            //Console.WriteLine(emp_101);
            //EmpDetails.GetEmp(6);

            //sort function 
            //EmpDetails.SortBySalary();

            //emp from name 
            //EmpDetails.GetEmpFromName();

            //emp from id
            /*string input;
            int empid;

            Console.WriteLine("Enter empid: ");
            input = Console.ReadLine();
            while (!int.TryParse(input, out empid))
            {
                Console.Write("Please enter an integer: ");
                input = Console.ReadLine();
            }
            EmpDetails.PrintEmpFromID(empid);*/

            //older emps
            EmpDetails.OlderEmployees();
        }


    }

    public partial class EmployeeDetails
    {
        Dictionary<int, Employee> EmpDetails;
        List<Employee> EmpList;
        public EmployeeDetails()
        {
            EmpDetails = new Dictionary<int, Employee>();
            EmpList = new List<Employee>();
        }

        public Dictionary<int,Employee> empdict { get => EmpDetails;}

        public void AddEmployee(Employee emp)
        {
            if (emp != null)
            {
                EmpDetails[emp.Id] = emp;
                EmpList.Add(emp);
            } else
            {
                Console.WriteLine("Employee cannot be null.");
            }
        }

        public Employee GetEmp(int empid)
        {
            if (EmpDetails.ContainsKey(empid) && empid != null)
            {
                return EmpDetails[empid];
            }
            else
            {
                return null;
            }
        }


        public void SortBySalary()
        {
            EmpList.Sort();
            foreach (Employee emp in EmpList)
            {
                Console.WriteLine(emp);
                Console.WriteLine();
            }

        }

        public void PrintEmpFromID(int empid)
        {

            var emp = EmpList.Where(n => n.Id == empid).FirstOrDefault();

            if (emp != null)
            {
                Console.WriteLine(emp);
            } else
            {
                Console.WriteLine("Employee not found.");
            }
        }

        public void GetEmpFromName()
        {
            string name;
            Console.Write("Enter name of Employee: ");
            name = Console.ReadLine();

            var emps = EmpList.Where(n => n.Name == name).ToList();

            if (emps != null)
            {
                foreach (Employee emp in emps)
                {
                    Console.WriteLine(emp);
                    Console.WriteLine();
                }

            }
            else
            {
                Console.WriteLine("No Employee found with that name.");
            }
        }

        public void OlderEmployees()
        {
            string input;
            int empid;
            Console.WriteLine("Enter employee id: ");
            input = Console.ReadLine();

            while (!int.TryParse(input,out empid))
            {
                Console.WriteLine("Please enter valid integer");
                input = Console.ReadLine();
            }


            if (!EmpDetails.ContainsKey(empid))
            {
                Console.WriteLine("Employee does not exist");
            }
            else
            {
                var olderemps = EmpList.Where(n => n.Age > EmpDetails[empid].Age).ToList();

                if (olderemps.Count != 0)
                {
                    Console.WriteLine($"Employees that are older than employee {empid}");
                    foreach (Employee emp in olderemps)
                    {
                        Console.WriteLine(emp);
                        Console.WriteLine();
                    }

                }
                else
                {
                    Console.WriteLine("This employee is the oldest");

                }
            }

        }
    }
}
