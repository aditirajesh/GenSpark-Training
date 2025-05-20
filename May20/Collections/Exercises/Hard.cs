using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicsofCollections.Exercises
{
    internal class Hard
    {
        public static void Run()
        {

            bool condition = true;
            string input;
            int choice;
            EmployeeDetails details = DummyData();

            while (condition)
            {
                Console.WriteLine();
                Console.WriteLine(@"MENU 
                1: Print all Employee Details
                2: Create and Add Employee
                3: Modify Details of an Employee
                4: Print Employee given ID
                5: Delete Employee
                6: Quit");
                Console.Write("Choose Your option:");
                input = Console.ReadLine();

                while (!int.TryParse(input, out choice) || !(choice >= 1 && choice <= 6))
                {
                    Console.Write("Invalid Input. Please enter numbers from 1-6: ");
                    input = Console.ReadLine();
                }

                if (choice != 6)
                {
                    Menu(details,choice);
                    condition = true;
                } else
                {
                    Console.WriteLine("Goodbye!");
                    condition = false;
                }
            }
        }

        static EmployeeDetails DummyData()
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

            return EmpDetails;
        }

        static void Menu(EmployeeDetails EmpDetails, int choice)
        {
            
            switch (choice)
            {
                case 1:
                    EmpDetails.PrintEmployees();
                    break;

                case 2:
                    EmpDetails.CreateEmployee();
                    break;

                case 3:
                    string input1, input2;
                    int empid_modify, choice_modify;
                    Console.Write("Enter empid of Employee you want to modify: ");
                    input1 = Console.ReadLine();

                    while (!int.TryParse(input1, out empid_modify))
                    {
                        Console.WriteLine("Invalid input. Please enter integer.");
                        input1 = Console.ReadLine();
                    }

                    Console.WriteLine(@"Enter 1 to change name
                    2 to change age
                    3 to change salary: ");
                    input2 = Console.ReadLine();
                    while (!int.TryParse(input2, out choice_modify) || !(choice_modify >= 1 && choice_modify <= 3))
                    {
                        Console.WriteLine("Invalid input. Please enter integer from 1 to 3");
                        input2 = Console.ReadLine();
                    }

                    EmpDetails.ModifyEmployee(empid_modify,choice_modify);
                    break;

                case 4:
                    string input3;
                    int empid_get;
                    Console.WriteLine("Enter ID: ");
                    input3 = Console.ReadLine();
                    while (!int.TryParse(input3, out empid_get))
                    {
                        Console.Write("Please enter valid integer");
                        input3 = Console.ReadLine();
                    }
                    EmpDetails.PrintEmpFromID(empid_get);
                    break;

                case 5:
                    string input4;
                    int empid_delete;
                    Console.WriteLine("Enter empid who you want to delete: ");
                    input4 = Console.ReadLine();

                    while (!int.TryParse(input4, out empid_delete))
                    {
                        Console.WriteLine("Invalid Input. Please enter valid ID: ");
                        input4 = Console.ReadLine();
                    }
                    EmpDetails.DeleteEmployee(empid_delete);
                    break;

            }

        }

    }

    public partial class EmployeeDetails
    {
        public void PrintEmployees()
        {
            Console.WriteLine("Printing all the employees...");
            foreach(Employee emp in EmpDetails.Values)
            {
                Console.WriteLine(emp);
                Console.WriteLine();
            }
        }

        public void CreateEmployee()
        {

            Employee new_emp = new Employee();
            new_emp.TakeEmployeeDetailsFromUser();
            EmpDetails[new_emp.Id] = new_emp;
            EmpList.Add(new_emp);
            Console.WriteLine("Successfully added employee!");
            Console.WriteLine();
        }

        public void ModifyEmployee(int empid,int choice)
        {
            
            Employee emp;

            if (!EmpDetails.ContainsKey(empid))
            {
                Console.WriteLine("Employee not found");
            } else
            {
                emp = EmpDetails[empid];
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Enter new name: ");
                        string name = Console.ReadLine();
                        emp.Name = name;
                        break;
                    case 2:
                        Console.WriteLine("Enter age: ");
                        string input = Console.ReadLine();
                        int age;
                        while (!int.TryParse(input, out age))
                        {
                            Console.WriteLine("Please enter valid number");
                            input = Console.ReadLine();
                        }
                        emp.Age = age;
                        break;
                    case 3:
                        Console.WriteLine("Enter Salary: ");
                        string input3 = Console.ReadLine();
                        int salary;
                        while (!int.TryParse(input3, out salary))
                        {
                            Console.WriteLine("Please enter valid number");
                            input3 = Console.ReadLine();
                        }
                        emp.Salary = salary;
                        break;

                }
                Console.WriteLine("Successfully updated.");

            }
        }

        public void DeleteEmployee(int empid)
        {

            if (EmpDetails.ContainsKey(empid))
            {
                Employee emp = EmpDetails[empid];
                EmpDetails.Remove(empid);
                emp = null;

                Console.WriteLine("Successfully deleted employee");

            } else
            {
                Console.WriteLine("Employee does not exist.");
            }

        }
    }
}
