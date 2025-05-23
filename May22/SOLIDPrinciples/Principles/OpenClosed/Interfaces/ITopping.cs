using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOLIDPrinciples.Principles.OpenClosed.Interfaces
{
    public interface ITopping
    {
        public string topping_name { get; }
        void Apply();
    }
}
