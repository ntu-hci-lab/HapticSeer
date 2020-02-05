using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample.API_Hook
{
    public class UnsupportedModuleException : Exception
    {
        public UnsupportedModuleException(string Module)
            : base("Unsupported Module: " + Module)
        {
        }
        public UnsupportedModuleException()
           : base("Unsupported Module")
        {
        }
    }
}
