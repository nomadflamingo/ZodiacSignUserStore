using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZodiacSignUserStore.Exceptions
{
    internal class TooOldBirthDateException : ArgumentException
    {
        public TooOldBirthDateException() : base("Your Date of Birth is too far in the past") { }
    }
}
