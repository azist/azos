/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azos.Collections
{
  /// <summary>
  ///  Represents list that rises change events
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class EventedList<TData, TContext> : EventedCollectionBase<TContext>, IList<TData>
  {
    #region Inner Types

    /// <summary>
    /// Describes changes in evented list
    /// </summary>
    public enum ChangeType
    {
      Insert,
      RemoveAt,
      Set,
      Add,
      Clear,
      Remove
    }


    public delegate void ChangeHandler(EventedList<TData, TContext> list,
                                       ChangeType change,
                                       EventPhase phase,
                                       int idx,
                                       TData item);

    #endregion


    #region .ctor

    /// <summary>
    ///  Initializes a new instance that is empty and has the default initial capacity.
    /// </summary>
    public EventedList() : base() { m_List = new List<TData>(); }

    /// <summary>
    ///  Initializes a new instance that is empty and has the default initial capacity.
    /// </summary>
    public EventedList(TContext context, bool contextReadOnly) : base(context, contextReadOnly) { m_List = new List<TData>(); }

    ///<summary>
    /// Initializes a new instance that contains elements copied from the specified collection and has sufficient
    ///  capacity to accommodate the number of elements copied.
    ///</summary>
    public EventedList(TContext context, bool contextReadOnly, IEnumerable<TData> collection) : base(context, contextReadOnly) { m_List = new List<TData>(collection); }

    ///<summary>
    /// Initializes a new instance that is empty and has the specified initial capacity.
    ///</summary>
    public EventedList(TContext context, bool contextReadOnly, int capacity) : base(context, contextReadOnly) { m_List = new List<TData>(capacity); }

    #endregion


    #region Private Fields

    private List<TData> m_List;

    #endregion


    #region Events

    [field: NonSerialized] public ChangeHandler ChangeEvent;

    #endregion

    #region IList Implementation

    public virtual int IndexOf(TData item) => m_List.IndexOf(item);

    public virtual void Insert(int index, TData item)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Insert, EventPhase.Before, index, item);
      m_List.Insert(index, item);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Insert, EventPhase.After, index, item);
    }

    public virtual void RemoveAt(int index)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.RemoveAt, EventPhase.Before, index, default(TData));
      m_List.RemoveAt(index);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.RemoveAt, EventPhase.After, index, default(TData));
    }

    public virtual TData this[int index]
    {
      get
      {
        return m_List[index];
      }
      set
      {
        CheckReadOnly();
        if (ChangeEvent != null) ChangeEvent(this, ChangeType.Set, EventPhase.Before, index, value);
        m_List[index] = value;
        if (ChangeEvent != null) ChangeEvent(this, ChangeType.Set, EventPhase.After, index, value);
      }
    }

    public virtual void Add(TData item)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Add, EventPhase.Before, -1, item);
      m_List.Add(item);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Add, EventPhase.After, -1, item);
    }

    public virtual void Clear()
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Clear, EventPhase.Before, -1, default(TData));
      m_List.Clear();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Clear, EventPhase.After, -1, default(TData));
    }

    public virtual bool Contains(TData item) => m_List.Contains(item);

    public virtual void CopyTo(TData[] array, int arrayIndex)
      => m_List.CopyTo(array, arrayIndex);

    public virtual int Count => m_List.Count;

    public virtual bool Remove(TData item)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Remove, EventPhase.Before, -1, item);
      var result = m_List.Remove(item);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Remove, EventPhase.After, -1, item);

      return result;
    }

    public virtual IEnumerator<TData> GetEnumerator()
      => m_List.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      => m_List.GetEnumerator();

    #endregion
  }


}
