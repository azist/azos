
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Data
{
    /// <summary>
    /// Describes what kind of modification was done
    /// </summary>
    public enum DocChangeType { Insert, Upsert, Update, Delete }

    /// <summary>
    /// Describes data document modification
    /// </summary>
    public struct DocChange
    {
      public DocChange(DocChangeType type, Doc doc, Access.IDataStoreKey key)
      {
          ChangeType = type;
          Doc = doc;
          Key = key;
      }

      public readonly DocChangeType ChangeType;
      public readonly Doc Doc;
      public readonly Access.IDataStoreKey Key;
    }
}
