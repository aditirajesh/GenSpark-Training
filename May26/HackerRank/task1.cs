using System;
using System.Collections.Generic;
using System.Linq;


//Notes Store
namespace Solution
{
    public class NotesStore
    {
        public List<Note> notes;
        public List<string> ValidStates = new List<string>{"complete","active","others"};

        public NotesStore()
        {
            notes = new List<Note>();
        }

        public void AddNote(String state, String name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Name cannot be empty");
            }
            if (!ValidStates.Contains(state))
            {
                throw new Exception($"Invalid state {state}");
            }

            notes.Add(new Note(name,state));
        }
        public List<String> GetNotes(String state)
        {
            if (!ValidStates.Contains(state))
            {
                throw new Exception($"Invalid state {state}");
            }

            return notes.Where(x => x.State == state).Select(x => x.Name).ToList();
        }

        public class Note {
            public string Name { get; set; }
            public string State { get; set; }

            public Note(string name, string state) {
                Name = name;
                State = state;
            }
        }

        public class Solution
        {
            public void Main()
            {
                var notesStoreObj = new NotesStore();
                var n = int.Parse(Console.ReadLine());
                for (var i = 0; i < n; i++)
                {
                    var operationInfo = Console.ReadLine().Split(' ');
                    try
                    {
                        if (operationInfo[0] == "AddNote")
                            notesStoreObj.AddNote(operationInfo[1], operationInfo.Length == 2 ? "" : operationInfo[2]);
                        else if (operationInfo[0] == "GetNotes")
                        {
                            var result = notesStoreObj.GetNotes(operationInfo[1]);
                            if (result.Count == 0)
                                Console.WriteLine("No Notes");
                            else
                                Console.WriteLine(string.Join(",", result));
                        }
                        else
                        {
                            Console.WriteLine("Invalid Parameter");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }
                }
            }
        }
    }
}