
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Glue.Protocol
{
    /// <summary>
    /// Specifies additional options for call like timeouts
    /// </summary>
    public struct CallOptions
    {
        public int DispatchTimeoutMs;
        public int TimeoutMs;
    }
}
