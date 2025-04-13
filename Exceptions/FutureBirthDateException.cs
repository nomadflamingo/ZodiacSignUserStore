using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZodiacSignUserStore.Exceptions
{
    internal class FutureBirthDateException : ArgumentException
    {
        public FutureBirthDateException() : base("Your Date of Birth cannot be in the future") { }
    }
}
