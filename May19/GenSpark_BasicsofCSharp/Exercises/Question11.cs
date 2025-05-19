using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question11
    {
        public static void Run()
        {
            int[][] SudokuBoard = new int[9][];
            int valid_rows = 0;
            for (int i=0;i<9;i++)
            {
                Console.WriteLine(" ");
                Console.WriteLine($"ROW {i + 1}");
                int[] row = Question10.Run();
                if (Question10.CheckRow(row))
                {
                    valid_rows++;
                }
                
            }

            if (valid_rows == 9)
            {
                Console.WriteLine("Congrats! The entire board is valid");
            } else
            {
                Console.WriteLine("Board is invalid, please try again");
            }

        }
    }
}
