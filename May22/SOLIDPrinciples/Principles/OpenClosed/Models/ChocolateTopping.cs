using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOLIDPrinciples.Principles.OpenClosed.Interfaces;

namespace SOLIDPrinciples.Principles.OpenClosed.Models
{
    public class ChocolateTopping: ITopping
    {
        public string topping_name => "Chocolate";

        public void Apply()
        {
            Console.WriteLine("Popcorn is dripped into Rich, Dark melted chocolate.... ");
            Thread.Sleep(1000);
        }
    }
}
