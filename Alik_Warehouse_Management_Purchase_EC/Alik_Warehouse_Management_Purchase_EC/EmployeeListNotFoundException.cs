using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_Warehouse_Management_Purchase_EC
{
    class EmployeeListNotFoundException : Exception
    {
        public EmployeeListNotFoundException()
        {
        }

        public EmployeeListNotFoundException(string message)
            : base(message)
        {
        }

        public EmployeeListNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
