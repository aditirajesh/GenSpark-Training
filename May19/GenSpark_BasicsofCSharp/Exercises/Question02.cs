using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question02
    {
        public static void Run()
        {
            int num1, num2;
            Console.WriteLine("Enter number 1:");
            string input1 = Console.ReadLine();
            Console.WriteLine("Enter number 2:");
            string input2 = Console.ReadLine();

            if (int.TryParse(input1, out num1) && int.TryParse(input2, out num2))
            {
                PrintMax(num1, num2);
            }
            else
            {
                Console.WriteLine("Invalid type. please only enter integers");
            }
        }

        static void PrintMax(int num1, int num2)
        {
            if (num1 != num2)
            {
                Console.WriteLine($"The larger number is: {Math.Max(num1, num2)}");
            }
            else
            {
                Console.WriteLine("Both numbers are equal");
            }
        }
    }
}
