using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DesignPatterns.Creational;

namespace DesignPatterns
{
    public class Program
    {
        public static void Main(string[] args) {
            Singleton FileSystem = Singleton.GetInstance();
            FileSystem.GetFile("C:\\Users\\arajesh\\Desktop\\GensparkTraining\\May23\\DesignPatterns\\Creational\\example.txt");
        }

    }
}