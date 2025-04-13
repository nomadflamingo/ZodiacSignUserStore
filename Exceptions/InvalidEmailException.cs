using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZodiacSignUserStore.Exceptions
{
    internal class InvalidEmailException : ArgumentException
    {
        public InvalidEmailException() : base("The provided email is not in a valid format.") { }
    }
}
