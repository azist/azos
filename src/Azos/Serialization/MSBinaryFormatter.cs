
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace Azos.Serialization
{
    /// <summary>
    /// Implements ISerializer with Ms BinaryFormatter
    /// </summary>
    public class MSBinaryFormatter : ISerializer
    {
      private BinaryFormatter m_Formatter = new BinaryFormatter();


      public void Serialize(System.IO.Stream stream, object root)
      {
          m_Formatter.Serialize(stream, root);
      }

      public object Deserialize(System.IO.Stream stream)
      {
          return m_Formatter.Deserialize(stream);
      }


      public bool IsThreadSafe
      {
        get { return false; }
      }
    }

}
