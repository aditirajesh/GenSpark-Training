using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesignPatterns.Creational.Interfaces;

namespace DesignPatterns.Creational.Models
{
    public class TextFileHandling: IFileHandling
    {
        public TextFileHandling() { }
        public void ReadFile(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new StreamReader(fs, leaveOpen: true);
            {
                Console.WriteLine("The contents of the file are: ");
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }

        }

        public void WriteFile(FileStream fs)
        {
            using StreamWriter writer = new StreamWriter(fs, leaveOpen: true);
            {
                Console.WriteLine("Enter content line by line. Type blank to stop: ");
                string input = Console.ReadLine();
                while (!string.IsNullOrWhiteSpace(input))
                {
                    writer.WriteLine(input);
                    input = Console.ReadLine();
                }
                writer.Flush();
            }
            Console.WriteLine("File has been successfully written into");

        }



    }
}
