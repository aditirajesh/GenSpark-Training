using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentApplication.Exceptions
{
    public class CollectionEmptyException: Exception
    {
        private string _message = "Collection is Empty.";
        public CollectionEmptyException(string msg)
        {
            _message = msg;
        }

        public override string Message => _message;
    }
}
