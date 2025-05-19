using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question09
    {
        public static void Run()
        {
            string secret = "GAME";
            bool success = false;
            int attempts = 0;
            while (!success){
                Console.Write("Enter 4-letter guess: ");
                string input = Console.ReadLine();
                attempts++;

                if (!CheckString(input, secret))
                {
                    Console.WriteLine("Guess must be a four-letter word, Please try again.");
                    success = false;

                } else
                {
                    success = GuessGame(input.ToUpper(), secret);
                }                
                
            }
            Console.WriteLine($"Total number of attempts: {attempts}");

        }

        static bool CheckString(string input, string secret)
        {
            if (string.IsNullOrEmpty(input) || secret.Length != input.Length)
            {
                return false;
            }
            return true;
        }

        static bool GuessGame(string input, string secret)
        {
            int Bulls = 0;
            int Cows = 0;

            if (secret.Equals(input))
            {
                Bulls = secret.Length;
                Cows = 0;
                Console.WriteLine($"{secret} {input} {Bulls} Bulls, {Cows} Cows EXACT MATCH!");
                return true;
            } else
            {
                string misplaced = "";
                string correctpos = "";
                Dictionary<char,int> found = new Dictionary<char,int>();
                for (int i=0; i<secret.Length; i++)
                {
                    if (secret[i] == input[i]) {
                        Bulls++;
                        correctpos = string.Concat(correctpos, input[i]);
                        found.Add(input[i], 1);
                    } else if (!found.ContainsKey(input[i]) && secret.Contains(input[i])){
                        Cows++;
                        misplaced = string.Concat(misplaced, input[i]);
                        found.Add(input[i], 1);
                    } else if (found.ContainsKey(input[i]) && found[input[i]] < secret.Count(n=> n == input[i])) {
                        Cows++;
                        misplaced = string.Concat(misplaced, input[i]);
                        found[input[i]]++;
                    }
                }
                

                if ((correctpos.Length + misplaced.Length) == secret.Length)
                {
                    Console.WriteLine($" {input} {Bulls} Bulls, {Cows} Cows {correctpos} right, {misplaced} misplaced");

                } else
                {
                    Console.WriteLine($"{input} {Bulls} Bulls, {Cows} Cows {correctpos} right, {misplaced} misplaced, rest wrong");

                }

                return false;
            }
        }
    }
}
