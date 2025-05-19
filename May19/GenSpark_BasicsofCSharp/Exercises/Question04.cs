using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    public class Question04
    {
        public static void Run()
        {
            const int attempts = 3;
            string username, password;
            bool success;

            for (int i=0;i<attempts;i++)
            {
                Console.WriteLine("Enter your username: ");
                username = Console.ReadLine();
                Console.WriteLine("Enter your password: ");
                password = Console.ReadLine();
                success = CheckLogin(username, password);
                if (success)
                {
                    Console.WriteLine("Login valid!");
                    return;
                } else
                {
                    Console.WriteLine($"Invalid, please try again. attempts left {attempts-i-1}");
                }

            }
            Console.WriteLine("Invalid attempts for 3 times. Exiting...");

            

        }

        static bool CheckLogin(string username, string password)
        {
            return username == "Admin" && password == "pass";
        }
    }
}
