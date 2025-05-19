using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question05
    {
        public static void Run()
        {
            int number;
            string input;
            int divisible_count = 0;

            for (int i=0;i<10;i++)
            {
                Console.Write("Enter a number: ");
                input = Console.ReadLine();

                if (!int.TryParse(input,out number)) {
                    Console.WriteLine("Input must be a number");
                    break;
                } else
                {
                    if (number%7 == 0)
                    {
                        divisible_count++;
                    }
                }

            }

            Console.WriteLine($"Number of numbers divisible by 7 is: {divisible_count}");

        }

    }
}
