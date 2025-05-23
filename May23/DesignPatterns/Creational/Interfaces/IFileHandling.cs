using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPatterns.Creational.Interfaces
{
    public interface IFileHandling
    {
        void WriteFile(FileStream fs);
        void ReadFile(FileStream fs);
    }
}
