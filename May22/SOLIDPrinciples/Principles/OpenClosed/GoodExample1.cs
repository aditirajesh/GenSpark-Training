using SOLIDPrinciples.Principles.OpenClosed.Interfaces;
using SOLIDPrinciples.Principles.OpenClosed.Models;
using SOLIDPrinciples.Principles.OpenClosed.Repository;
using SOLIDPrinciples.Principles.OpenClosed.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOLIDPrinciples.Principles.OpenClosed
{
    public class GoodExample1
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
            var toppingDict = new Dictionary<string, ITopping>
            {
                { "Butter", new ButterTopping() },
                { "Masala", new MasalaTopping() },
                { "Chocolate", new ChocolateTopping() }
            };

            IToppingRepository popcornRepository = new ToppingRepository(toppingDict);
            _popcornPreparation = new PopcornPreparation();
            _popcornmaking = new PopcornMaking();
            _popcornFinalizer = new PopcornFinalizer(popcornRepository);
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
        public void ChooseKernel()
        {
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
        public void InsertIntoMicrowave()
        {
            Console.WriteLine("Inserting bag into the microwave...");
            Console.WriteLine();
            Thread.Sleep(200);
        }

    }

    public class PopcornMaking
    {
        public void SetTimer()
        {
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
    }

    public class PopcornFinalizer
    {
        private readonly IToppingRepository _toppingRepo;
        public void RemoveBag()
        {
            Console.WriteLine("Removing bag, its piping hot!...");
            Console.WriteLine();
            Thread.Sleep(100);
        }

        public PopcornFinalizer(IToppingRepository toppingRepo)
        {
            _toppingRepo = toppingRepo;
        }

        public void AddToppings()
        {
            var toppings = _toppingRepo.DisplayAllToppings();
            Console.WriteLine("Available Toppings:");
            foreach (var topping in toppings)
            {
                Console.WriteLine($"- {topping}");
            }

            Console.Write("Enter your topping choices (comma-separated): ");
            string input = Console.ReadLine();
            var selected_toppings = input.Split(',')
                                .Select(t => t.Trim())
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .ToList();

            foreach (var name in selected_toppings)
            {
                var topping = _toppingRepo.GetTopping(name);
                if (topping != null)
                {
                    Console.WriteLine($"Applying {topping.topping_name}...");
                    topping.Apply();
                }
                else
                {
                    Console.WriteLine($"'{name}' is not a valid topping.");
                }
            }

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
