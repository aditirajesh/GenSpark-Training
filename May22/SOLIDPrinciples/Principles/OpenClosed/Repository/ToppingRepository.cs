using SOLIDPrinciples.Principles.OpenClosed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOLIDPrinciples.Principles.OpenClosed.Repository
{
    public class ToppingRepository : IToppingRepository
    {
        private Dictionary<string, ITopping> _topping_dictionary;

        public ToppingRepository(Dictionary<string, ITopping> toppings)
        {
            _topping_dictionary = toppings;
        }

        public ICollection<string>? DisplayAllToppings()
        {
            if (_topping_dictionary != null && _topping_dictionary.Count != 0)
            {
                return _topping_dictionary.Keys;
            }
            else
            {
                return null;
            }
        }

        public ITopping GetTopping(string name)
        {
            if (string.IsNullOrEmpty(name) || !_topping_dictionary.ContainsKey(name))
            {
                return null;
            }
            else
            {
                return _topping_dictionary[name];
            }

        }
    }
}
