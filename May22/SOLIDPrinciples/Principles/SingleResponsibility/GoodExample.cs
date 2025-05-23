using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOLIDPrinciples.Principles.SingleResponsibility
{
    public class GoodExample
    {
        public static void Run()
        {
            PopcornController controller = new PopcornController();
            controller.MakePopcorn();
        }

    }

    public class PopcornController
    {
        private PopcornPreparation _popcornPreparation;
        private PopcornMaking _popcornmaking;
        private PopcornFinalizer _popcornFinalizer;

        public PopcornController()
        {
            _popcornPreparation = new PopcornPreparation();
            _popcornmaking = new PopcornMaking(); 
            _popcornFinalizer = new PopcornFinalizer();
        }

        public void MakePopcorn()
        {
            _popcornPreparation.ChooseKernel();
            _popcornPreparation.InsertIntoMicrowave();

            _popcornmaking.SetTimer();
            _popcornmaking.StartMicrowave();
            _popcornmaking.StopMicrowave();

            _popcornFinalizer.RemoveBag();
            _popcornFinalizer.AddToppings();
            _popcornFinalizer.Serve();
        }
    }

    public class PopcornPreparation
    {
        public void ChooseKernel() {
            string user_choice;
            int kernel;
            Console.Write("Please Enter 1 for Mushroom Kernels, 2 for Butterfly Kernels: ");
            while (!int.TryParse(Console.ReadLine(), out kernel) || !(kernel >= 1 && kernel <= 2))
            {
                Console.Write("Invalid Input. Please enter 1 or 2: ");
            }
            user_choice = kernel == 1
            ? "You have chosen Mushroom Kernels"
            : "You have chosen Butterfly Kernels!";

            Console.Write(user_choice);
            Console.WriteLine();
            Thread.Sleep(200);
        }
        public void InsertIntoMicrowave() {
            Console.WriteLine("Inserting bag into the microwave...");
            Console.WriteLine();
            Thread.Sleep(200);
        }

    }

    public class PopcornMaking
    {
        public void SetTimer() {
            int time;
            Console.Write("Please Enter time(seconds): ");
            while (!int.TryParse(Console.ReadLine(), out time) || time <= 0)
            {
                Console.Write("Invalid Input. Please enter positive value of time ");
            }
            Console.WriteLine($"Setting timer for {time} seconds.....");
            Console.WriteLine();
            Thread.Sleep(200);
        }
        public void StartMicrowave() {
            Console.WriteLine("Starting the microwave....");
            Console.WriteLine();
            Thread.Sleep(300);
        }
        public void StopMicrowave() {
            Console.WriteLine("Stopping the microwave....");
            Console.WriteLine();
            Thread.Sleep(100);
        }
    }

    public class PopcornFinalizer
    {
        public void RemoveBag() {
            Console.WriteLine("Removing bag, its piping hot!...");
            Console.WriteLine();
            Thread.Sleep(100);
        }
        public void AddToppings() {
            int butter_option, masala_option;
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
        public void Serve() {
            Console.WriteLine("Your popcorn is ready!");
            Console.WriteLine();
        }
    }
}
