
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Data
{
  /// <summary>
  /// Checks for document equality using doc.Equals() method. Use DocEqualityComparer.Instance
  /// </summary>
  public sealed class DocEqualityComparer : EqualityComparer<Doc>
  {
      public static readonly DocEqualityComparer Instance = new DocEqualityComparer();

      private DocEqualityComparer() {}


      public override bool Equals(Doc x, Doc y)
      {
        if (x==null && y==null) return true;
        if (x==null) return false;

        return x.Equals(y);
      }

      public override int GetHashCode(Doc doc)
      {
        if (doc == null) return 0;
        return doc.GetHashCode();
      }
  }
}
