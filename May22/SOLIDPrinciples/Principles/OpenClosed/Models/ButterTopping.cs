using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOLIDPrinciples.Principles.OpenClosed.Interfaces;
namespace SOLIDPrinciples.Principles.OpenClosed.Services
{
    public class ButterTopping: ITopping
    {
        public string topping_name => "Butter";
        
        public void Apply()
        {
            Console.WriteLine("Fresh, hot butter is drizzled down on the popcorn...");
            Thread.Sleep(1000);
        }
    }
}
