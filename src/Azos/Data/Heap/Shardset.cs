/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Azos.Data.Heap
{
  public sealed class Router
  {
    public sealed class Set : IEnumerable<KeyValuePair<int, IHeapNode>>
    {
      private DateTime m_EffectiveUtc;
      private KeyValuePair<int, IHeapNode>[] m_Set;

      public DateTime EffectiveUtc => m_EffectiveUtc;
      public IHeapNode this[int i] => m_Set[i.KeepBetween(0, m_Set.Length-1)].Value;

      public IEnumerator<KeyValuePair<int, IHeapNode>> GetEnumerator()
        => m_Set.Cast<KeyValuePair<int, IHeapNode>>().GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => m_Set.GetEnumerator();
    }



    public Set this[int idx]
    {
      get{ return null; }
    }

    public Set this[DateTime asOfUtc]
    {
      get{ return null;}
    }
  }
}
