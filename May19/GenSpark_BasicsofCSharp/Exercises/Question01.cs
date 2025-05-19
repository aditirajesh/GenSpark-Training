using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question01
    {
        public static void Run()
        {
            string name;
            Console.Write("Enter your name: ");
            name = Console.ReadLine();
            if (string.IsNullOrEmpty(name)) {
                Console.WriteLine("Please enter a valid name");
            } else
            {
                GreetUser(name);
            }

        }

        static void GreetUser(string name)
        {
            Console.WriteLine($"Greetings, {name}");

        }
    }

}
