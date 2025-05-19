using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question07
    {
        public static void Run()
        {
            int lenarray;
            Console.WriteLine("Enter length of array: ");
            string input = Console.ReadLine();

            if (!int.TryParse(input,out lenarray))
            {
                Console.Write("Please enter an integer.");
            } else if(lenarray <= 0)
            {
                Console.WriteLine("Array length must be a positive non-zero value");
            } else
            {
                int[] numbers = new int[lenarray];
                for(int i=0;i<lenarray;i++)
                {
                    Console.Write($"Enter number {i + 1}: ");
                    int number = Convert.ToInt32(Console.ReadLine());
                    numbers[i] = number;
                }

                int[] RotatedArray = RotateArray(numbers);
                Console.WriteLine("The original array is: ");
                Console.WriteLine(string.Join(", ", numbers));
                Console.WriteLine("The Rotated Array is:");
                Console.WriteLine(string.Join(", ", RotatedArray));

            }


        }

        static int[] RotateArray(int[] numbers)
        {
            int[] RotatedArray = new int[numbers.Length];
            for (int i=0; i<numbers.Length; i++)
            {
                RotatedArray[i] = numbers[(i + 1) % numbers.Length];
            }
            return RotatedArray;
        }
    }
}
