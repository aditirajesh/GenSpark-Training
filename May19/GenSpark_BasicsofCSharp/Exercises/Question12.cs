using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GenSpark_BasicsofCSharp.Exercises
{
    internal class Question12
    {
        public static void Run()
        {
            string input1,input2;
            Console.Write("Enter lowecase string, with no whitspaces or special chars: ");
            input1 = Console.ReadLine();
            Console.Write("Enter chars to shift by: ");
            input2 = Console.ReadLine();
            if (Regex.IsMatch(input1,@"^[a-z]+$") && CheckInput(input2))
            {
                int shift = Convert.ToInt32(input2);
                string encrypted = Encrypt(input1,shift);
                string decrypted = Decrypt(encrypted,shift);
                Console.WriteLine($"Shift: {shift}");
                Console.WriteLine($"Encrypted: {encrypted}");
                Console.WriteLine($"Decrypted: {decrypted}");
            } else
            {
                Console.WriteLine("String must be 1.Lowercase, 2.no whitespace, 3.no numbers or symbols");
                Console.WriteLine("number to shift by must be a positive integer");
            }
        }

        static string Encrypt(string input,int shift)
        {
            string encrypted_string = "";
            for(int i=0;i<input.Length;i++)
            {
                char encrypted_char = (char)(((input[i] - 'a' + shift) % 26) + 'a');
                encrypted_string += encrypted_char;
            }
            return encrypted_string;
        }

        static string Decrypt(string encrypted_string, int shift)
        {
            string decrypted_string = "";
            for (int i=0;i<encrypted_string.Length;i++)
            {
                char decrypted_char = (char)(((encrypted_string[i] - 'a' - shift+26) % 26)+'a');
                decrypted_string += decrypted_char;
            }
            return decrypted_string;
        }

        static bool CheckInput(string input)
        {
            int result;
            if (!int.TryParse(input, out result))
            {
                return false;
            } else if(result < 0)
            {
                return false;
            } else
            {
                return true;
            }
        }
    }
}
