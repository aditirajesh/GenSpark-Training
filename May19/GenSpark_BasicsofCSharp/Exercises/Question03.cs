using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    public class Question03
    {
        public static void Run()
        {
            double num1, num2;
            string operation, input1, input2;

            Console.WriteLine("Enter number 1: ");
            input1 = Console.ReadLine();
            Console.WriteLine("Enter number 2: ");
            input2 = Console.ReadLine();
            Console.WriteLine("Enter operation +,-,*,/: ");
            operation = Console.ReadLine();

            if (double.TryParse(input1, out num1) && double.TryParse(input2, out num2)) {
                DoOperations(num1, num2, operation);
            } else
            {
                Console.WriteLine("Must enter numbers. please try again");
            }
           

        }

        static void DoOperations(double num1, double num2, string operation)
        {
            double result;
            switch(operation)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "*":
                    result = num1 * num2;
                    break;
                case "/":
                    if (num2 == 0)
                    {
                        Console.WriteLine("Division by 0 not permitted");
                            return;
                    } else
                    {
                        result = num1 / num2;
                    }
                    break;
                default:
                    Console.WriteLine("Operation provided is invalid. Try again");
                    return;

            }
            Console.WriteLine($"{num1}{operation}{num2}={result}");
        }
    }
}
