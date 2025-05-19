using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question10
    {

        public static int[] Run()
        {
            int[] SudokuRow = new int[9];
            for (int i = 0; i < 9; i++)
            {
                Console.Write($"Enter number{i + 1}: ");
                string input = Console.ReadLine();

                while (!CheckInput(input))
                {
                    Console.WriteLine("Please enter valid input");
                    Console.Write("Enter number again: ");
                    input = Console.ReadLine();
                }

                int number = Convert.ToInt32(input);
                SudokuRow[i] = number;
            }

            bool validity = CheckRow(SudokuRow);
            if (validity)
            {
                Console.WriteLine("VALID");
            } else
            {
                Console.WriteLine("INVALID");
            }
            return SudokuRow;
        }

        public static bool CheckRow(int[] row)
        {
            int[] Sorted = row;
            Array.Sort(Sorted);
            if (Sorted.SequenceEqual(Enumerable.Range(1, 9)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool CheckInput(string input){
               int number;
               if (int.TryParse(input, out number))
               {
                   return true;
               }
               else
               {
                   return false;
               }
        }
    }
}
