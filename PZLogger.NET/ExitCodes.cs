using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZLogger.NET
{
    [Flags]
    public enum ExitCodes
    {
        Success = 0,
        UnknownError = 1,
    }
}