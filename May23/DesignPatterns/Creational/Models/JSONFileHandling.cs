using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using DesignPatterns.Creational.Interfaces;


namespace DesignPatterns.Creational.Models
{
    public class JSONFileHandling : IFileHandling
    {
        public JSONFileHandling() { }

        public void ReadFile(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            Console.WriteLine("Contents of the JSON file are: ");
            using (StreamReader reader = new StreamReader(fs, leaveOpen: true)) 
            {
                string json = reader.ReadToEnd();
                List<string>? entries = JsonSerializer.Deserialize<List<string>>(json);

                if (entries != null && entries.Count > 0)
                {
                    foreach (string entry in entries)
                    {
                        Console.WriteLine(entry);
                    }
                }

            }
        }

        public void WriteFile(FileStream fs)
        {
            using (StreamWriter writer = new StreamWriter(fs, leaveOpen: true))
            {
                Console.WriteLine("Enter content line by line. Type blank to stop: ");
                List<string> entries = new List<string>();
                string input = Console.ReadLine();

                while (!string.IsNullOrWhiteSpace(input))
                {
                    entries.Add(input);
                    input = Console.ReadLine();
                }

                string json_String = JsonSerializer.Serialize(entries);
                writer.WriteLine(json_String);
                writer.Flush();

            }

            Console.WriteLine("Wrote into json file");
        }
    }
}
