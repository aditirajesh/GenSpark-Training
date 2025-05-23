using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOLIDPrinciples.Principles.OpenClosed.Interfaces
{
    public interface IToppingRepository
    {
        ICollection<string>? DisplayAllToppings();
        ITopping GetTopping(string name);

    }
}
