
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Conf
{
      /// <summary>
      /// Implements configuration that can not be persisted/loaded anywhere - just stored in memory
      /// </summary>
      [Serializable]
      public sealed class MemoryConfiguration : Configuration
      {



        public override bool IsReadOnly
        {
          get { return false; }
        }
      }


}
