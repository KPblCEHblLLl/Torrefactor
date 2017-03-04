using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Torrefactor.Controllers
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException()
            : this("Concurrency rate is too high")
        {
        }

        public ConcurrencyException(string message) : base(message)
        {
        }

        public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}