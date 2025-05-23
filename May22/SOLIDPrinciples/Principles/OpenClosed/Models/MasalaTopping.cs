using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOLIDPrinciples.Principles.OpenClosed.Interfaces;


namespace SOLIDPrinciples.Principles.OpenClosed.Models
{
    public class MasalaTopping: ITopping
    {
        public string topping_name => "Masala";

        public void Apply()
        {
            Console.WriteLine("A mix of Spicy Hawaiian Barbeque, Cheddar Cheese and Sour Cream Onion is sprinkled on the popcorn");
            Thread.Sleep(1000);
        }
    }
}
