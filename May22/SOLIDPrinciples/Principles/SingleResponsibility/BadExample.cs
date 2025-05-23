using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Objects should have one, and only one reason to change. - this class  has way too many functions.

namespace SOLIDPrinciples.Principles.SingleResponsibility
{
    public class BadExample
    {

        public static void Run()
        {
            BadExample popcorn = new BadExample();
            popcorn.MakePopcorn();

        }

        public void MakePopcorn()
        {
            ChooseKernel();
            InsertIntoMicrowave();
            SetTimer();
            StartMicrowave();
            StopMicrowave();
            RemoveBag();
            AddToppings();
            Serve();

        }

        public void ChooseKernel()
        {
            string user_choice;
            int kernel;
            Console.Write("Please Enter 1 for Mushroom Kernels, 2 for Butterfly Kernels: ");
            while (!int.TryParse(Console.ReadLine(),out kernel)|| !(kernel >= 1 && kernel <= 2)) {
                Console.Write("Invalid Input. Please enter 1 or 2: ");
            }
            user_choice = kernel == 1
            ? "You have chosen Mushroom Kernels"
            : "You have chosen Butterfly Kernels!";

            Console.Write(user_choice);
            Console.WriteLine();
            Thread.Sleep(200);

        }

        public void InsertIntoMicrowave ()
        {
            Console.WriteLine("Inserting bag into the microwave...");
            Console.WriteLine();
            Thread.Sleep(200);

        }

        public void SetTimer()
        {
            int time;
            Console.Write("Please Enter time(seconds): ");
            while (!int.TryParse(Console.ReadLine(), out time) || time <= 0) {
                Console.Write("Invalid Input. Please enter positive value of time ");
            }
            Console.WriteLine($"Setting timer for {time} seconds.....");
            Console.WriteLine();
            Thread.Sleep(200);

        }

        public void StartMicrowave()
        {
            Console.WriteLine("Starting the microwave....");
            Console.WriteLine();
            Thread.Sleep(300);

        }

        public void StopMicrowave()
        {
            Console.WriteLine("Stopping the microwave....");
            Console.WriteLine();
            Thread.Sleep(100);

        }

        public void RemoveBag()
        {
            Console.WriteLine("Removing bag, its piping hot!...");
            Console.WriteLine();
            Thread.Sleep(100);

        }

        public void AddToppings()
        {
            int butter_option,masala_option;
            Console.Write("Do you want to add butter? 1 for yes, 2 for no: ");
            while (!int.TryParse(Console.ReadLine(), out butter_option) || !(butter_option <= 2 && butter_option >= 1))
            {
                Console.Write("Invalid Input. Please enter 1 or 2: ");
            }
            Console.WriteLine(butter_option == 1 ? "Adding butter..." : "No butter added.");
            Console.WriteLine();

            Console.Write("Do you want to add masala? 1 for yes, 2 for no: ");
            while (!int.TryParse(Console.ReadLine(), out masala_option) || !(masala_option <= 2 && masala_option >= 1))
            {
                Console.Write("Invalid Input. Please enter 1 or 2: ");
            }
            Console.WriteLine(masala_option == 1 ? "Adding masala: sour cream onion and peri peri!..." : "No masala added.");
            Console.WriteLine();
            Thread.Sleep(200);

        }

        public void Serve()
        {
            Console.WriteLine("Your popcorn is ready!");
            Console.WriteLine();

        }
    }

}
