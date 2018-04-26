using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Helpers
{
    public class TransferException : Exception
    {
        public string Content { get; set; }
    }
}
