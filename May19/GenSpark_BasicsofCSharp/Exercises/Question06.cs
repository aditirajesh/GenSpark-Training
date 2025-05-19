using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question06
    {

        public static void Run()
        {
            List<int> counted = new List<int>();

            int lenarray;
            Console.Write("Enter length of array: ");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out lenarray)) {
                Console.WriteLine("Please enter an integer as input.");
            } else
            {
                if (lenarray <= 0) {
                    Console.WriteLine("Please enter a positive integer as input");
                } else
                {
                    int[] numbers = new int[lenarray];
                    for (int i=0;i<lenarray;i++)
                    {
                        Console.Write($"Enter number {i + 1} ");
                        int number = Convert.ToInt32(Console.ReadLine());
                        numbers[i] = number;

                    }
                    Console.WriteLine("Counting Frequency: ");
                    GetCount(numbers);


                }
            }
        }

        static void GetCount(int[] numbers)
        {

            List<int> counted = new List<int>();
            for (int i=0; i<numbers.Length;i++)
            {
                if (!counted.Contains(numbers[i]))
                {
                    int count = numbers.Count(n => n == numbers[i]);
                    counted.Add(numbers[i]);
                    Console.WriteLine($"{numbers[i]} appear {count} time(s)");
                }
            }

            
        }


    }
}
