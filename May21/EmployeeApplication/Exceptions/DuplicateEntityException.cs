using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeApplication.Exceptions
{
    public class DuplicateEntityException: Exception
    {
        private string _message = "Duplicate entity found";
        public DuplicateEntityException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
        //Exception base class has a virtual property called message -
        //this tells it to return the custom message when exception called.
    }
}
