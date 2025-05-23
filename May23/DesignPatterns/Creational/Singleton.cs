using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DesignPatterns.Creational
{
    public sealed class Singleton //must be a sealed class to prevent inheritance
    {
        private Singleton() { } //constructor should be private

        private static Singleton? _instance=null; //the global singleton reference

        public static Singleton GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Singleton();
            }
            return _instance;
        }

        public void WriteFile(FileStream fs)
        {
            using StreamWriter writer = new StreamWriter(fs,leaveOpen:true);
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

        public void GetFile(string path)
        {
            if (path != null)
            {
                using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                {
                    WriteFile(fs);
                    ReadFile(fs);
                }
            } else
            {
                Console.WriteLine("File path cannot be null");
            }
        }

    }

    public class FileWriter()
    {
    }
}
