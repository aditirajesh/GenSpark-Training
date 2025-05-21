using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentApplication.Exceptions
{
    public class DuplicateEntityException: Exception
    {
        private string _message = "Duplicate Entity Present";
        public DuplicateEntityException(string msg) {
            _message = msg;
        }

        public override string Message => _message;
    }
}
