using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question08
    {
        public static void Run()
        {
            int lenarray1, lenarray2;
            string input1, input2;
            Console.Write("Enter length of array 1: ");
            input1 = Console.ReadLine();
            Console.Write("Enter length of array 2: ");
            input2 = Console.ReadLine();

            if (!int.TryParse(input1, out lenarray1) || !int.TryParse(input2, out lenarray2))
            {
                Console.Write("Please enter an integer as length.");
            } else if (lenarray1 <= 0 || lenarray2 <= 0)
            {
                Console.WriteLine("Array length must be a positive non-zero value");
            } else
            {
                Console.WriteLine("For ARRAY 1: ");
                int[] array1 = GetInputArray(lenarray1);
                Console.WriteLine("For ARRAY 2: ");
                int[] array2 = GetInputArray(lenarray2);

                int[] mergedarray = MergeArrays(array1, array2);
                Console.WriteLine($"Arrays:({string.Join(", ", array1)}), ({string.Join(", ", array2)})");
                Console.WriteLine("The Merged array is: ");
                Console.WriteLine(string.Join(", ", mergedarray));

            }
        }

        static int[] MergeArrays(int[] array1, int[] array2)
        {
            int[] mergedarray = new int[array1.Length + array2.Length];
            Array.Copy(array1, 0, mergedarray, 0, array1.Length);
            Array.Copy(array2, 0, mergedarray, array1.Length, array2.Length);

            return mergedarray;


        }

        static int[] GetInputArray(int lenarray)
        {
            int[] userarray = new int[lenarray];
            for(int i=0;i<lenarray;i++)
            {
                Console.WriteLine($"Enter number {i + 1}: ");
                int number = Convert.ToInt32(Console.ReadLine());
                userarray[i] = number;
            }
            return userarray;
        }
    }
}
