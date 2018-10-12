/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Collections
{
    /// <summary>
    /// Defines an enumerator with look-ahead capability
    /// </summary>
    public interface ILookAheadEnumerator<T> : IEnumerator<T>
    {
      bool HasNext { get; }
      T Next { get; }
    }

    /// <summary>
    /// Defines an enumerable with look-ahead capability
    /// </summary>
    public interface ILookAheadEnumerable<T> : IEnumerable<T>
    {
      ILookAheadEnumerator<T> GetLookAheadEnumerator();
    }

    /// <summary>
    /// Implements a look-ahead enumerator wrapped around a regular enumerator
    /// </summary>
    public sealed class LookAheadEnumerator<T> : ILookAheadEnumerator<T>
    {
      private static readonly IEnumerator<T> s_Empty = Enumerable.Empty<T>().GetEnumerator();

      private readonly IEnumerator<T> m_Enumerator;
      private bool m_Fetch;
      private bool m_HasNext;
      private bool m_BeforeStart;
      private T m_Current;

      public LookAheadEnumerator(IEnumerator<T> enumerator)
      {
        m_Fetch = false;
        m_HasNext = false;
        m_BeforeStart = true;
        m_Current = default(T);
        m_Enumerator = enumerator;
      }

      public bool HasNext
      {
        get
        {
          if (!m_Fetch)
          {
            if (m_HasNext)
              m_Current = m_Enumerator.Current;
            m_Fetch = true;
            m_HasNext = m_Enumerator.MoveNext();
          }
          return m_HasNext;
        }
      }

      public T Next
      {
        get
        {
          if (HasNext) return m_Enumerator.Current;

          throw new AzosException(StringConsts.INVALID_OPERATION_ERROR + "{0}.Next(!HasNext)".Args(GetType().FullName));
        }
      }

      public T Current
      {
        get
        {
          return !m_Fetch ? m_Enumerator.Current : m_BeforeStart ? s_Empty.Current : m_Current;
        }
      }

      public void Dispose()
      {
        m_Current = default(T);
        m_Enumerator.Dispose();
      }

      object IEnumerator.Current { get { return Current; } }

      public bool MoveNext()
      {
        if (!m_Fetch) return m_Enumerator.MoveNext();
        else
        {
          m_Fetch = false;
          m_BeforeStart = false;
          return m_HasNext;
        }
      }

      public void Reset()
      {
        m_Fetch = false;
        m_HasNext = false;
        m_BeforeStart = true;
        m_Current= default(T);
        m_Enumerator.Reset();
      }
    }


    /// <summary>
    /// Implements a look-ahead enumerator wrapped around a regular enumerator
    /// </summary>
    public sealed class LookAheadEnumerable<T> : ILookAheadEnumerable<T>
    {
      /// <summary> References source enumerable which this one is wrapped around </summary>
      public readonly IEnumerable<T> Source;

      public LookAheadEnumerable(IEnumerable<T> enumerable)   => Source = enumerable;

      public ILookAheadEnumerator<T> GetLookAheadEnumerator() => new LookAheadEnumerator<T>(Source.GetEnumerator());
      public IEnumerator<T> GetEnumerator()                   => GetLookAheadEnumerator();
      IEnumerator IEnumerable.GetEnumerator()                 => GetEnumerator();
    }

    /// <summary>
    /// Extensions methods for LookAheadEnumerable
    /// </summary>
    public static class LookAheadExtensions
    {
      public static ILookAheadEnumerable<T> AsLookAheadEnumerable<T>(this IEnumerable<T> enumerable) => new LookAheadEnumerable<T>(enumerable);
    }
}
