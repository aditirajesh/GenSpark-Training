using System;
using System.Collections.Generic;
using BasicsofCollections.Exercises;
using BasicsofCollections;
using System.ComponentModel.Design;

namespace BasicsofCollections.Exercises
{
    internal class Easy
    {
        public static void Run()
        {
            Dictionary<string, Employee> EmpDict = new Dictionary<string, Employee>();

            Employee emp1 = new Employee(101, 28, "Ramu", 45000);
            Employee emp2 = new Employee(102, 32, "Bimu", 52000);
            Employee emp3 = new Employee(103, 26, "Somu", 48000);
            Employee emp4 = new Employee(104, 35, "Gomu", 60000);
            Employee emp5 = new Employee(105, 29, "Vimu", 50000);

            EmpDict.Add(emp1.Name, emp1);
            EmpDict.Add(emp2.Name, emp2);
            EmpDict.Add(emp3.Name, emp3);
            EmpDict.Add(emp4.Name, emp4);
            EmpDict.Add(emp5.Name, emp5);

            EmployeePromotion promoted_emps = AddEmpsForPromotion(EmpDict);

            //promoted_emps.PrintPromotionEmployees();
            //promoted_emps.GetPromotionPos("Ramu");
            //promoted_emps.Trim_Excess();
            //promoted_emps.PrintPromotedEmployees();
        }

        static void DummyData()
        {

        }

        static EmployeePromotion AddEmpsForPromotion(Dictionary<string,Employee> empdict)
        {
            bool condition = true;
            string name;
            EmployeePromotion promoted_emps = new EmployeePromotion();


            while (condition)
            {
                Console.WriteLine();
                Console.Write("Enter employee name: ");
                name = Console.ReadLine();
                if (!empdict.ContainsKey(name) && !string.IsNullOrWhiteSpace(name))
                {
                    Console.Write("Employee does not exist ");
                    condition = true;
                } else if (empdict.ContainsKey(name)) {
                    promoted_emps.AddEmployee(empdict[name]);
                    condition = true;
                } else
                {
                    condition = false;

                }    

            }

            return promoted_emps;
        }

    }

    public class EmployeePromotion
    {
        List<Employee> EmployeesForPromotion;
        List<string> EmployeeNames;
        public EmployeePromotion()
        {
            this.EmployeesForPromotion = new List<Employee>();
            this.EmployeeNames = new List<string>();
        }

        public List<string> EmpNames { get => EmployeeNames; }

        public void AddEmployee(Employee employee)
        {
            EmployeesForPromotion.Add(employee);
            EmployeeNames.Add(employee.Name);
            EmployeesForPromotion.TrimExcess();
        }

        public void Trim_Excess()
        {
            Console.WriteLine($"Before trimming: {EmployeeNames.Capacity}");
            EmployeeNames.TrimExcess();
            Console.WriteLine($"After trimming: {EmployeeNames.Capacity}");
        }

        public void PrintPromotionEmployees()
        {
            Console.WriteLine("List of Employees for Promotion: ");
            if (EmployeeNames.Count > 0)
            {
                foreach (string empname in EmployeeNames)
                {
                    Console.WriteLine(empname);
                }
            }
            else
            {
                Console.Write("No employees in line for promotion");
            }
            Console.WriteLine();
        }

        public void GetPromotionPos(string empname)
        {
            if (EmployeeNames.Contains(empname))
            {
                Console.WriteLine($"{empname} is in position {EmployeeNames.IndexOf(empname)} for promotion");
            }
            else
            {
                Console.WriteLine($"{empname} is not in line for a promotion");
            }
            Console.WriteLine();
        }

        public void PrintPromotedEmployees()
        {
            Console.WriteLine("All Employees that have been promoted: ");
            List<string> sorted_emps = new List<string>(EmployeeNames);
            sorted_emps.Sort();

            foreach (string empname in sorted_emps)
            {
                Console.WriteLine(empname);
            }
            Console.WriteLine();
        }
    }
}