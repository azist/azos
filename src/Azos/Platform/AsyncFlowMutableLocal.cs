using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Azos.Platform
{
  /// <summary>
  /// Represents a mutable analog of AsyncLocal(T). The mutability is achieved by
  /// wrapping the mutable value in a fake class instance which internal AsyncLocal stores, however changes are done to the
  /// wrapped value, not the class instance which is immutable
  /// </summary>
  public sealed class AsyncFlowMutableLocal<T>
  {
    private class wrap<TData>{  public TData Data; }

    private readonly AsyncLocal<wrap<T>> m_Local;

    public AsyncFlowMutableLocal() => m_Local = new AsyncLocal<wrap<T>>();

    /// <summary>
    /// Provides access to mutable wrapped async local value
    /// </summary>
    public T Value
    {
      get
      {
        var v = m_Local.Value;
        if (v==null) return default(T);
        return v.Data;
      }

      set
      {
        var v = m_Local.Value;
        if (v==null) m_Local.Value = v = new wrap<T>();
        v.Data = value;
      }
    }
  }
}
