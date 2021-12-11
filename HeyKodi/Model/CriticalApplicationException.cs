using KodiRPC.RPC.Specifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Model
{
    public class CriticalApplicationException : Exception
    {
        public CriticalApplicationException(string message) : base(message)
        {
        }

        public CriticalApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
