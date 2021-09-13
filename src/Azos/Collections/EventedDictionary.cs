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
  /// Represents a dictionary that rises events
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class EventedDictionary<TKey, TValue, TContext> : EventedCollectionBase<TContext>, IDictionary<TKey, TValue>
  {
    #region Inner Types

    /// <summary>
    /// Describes changes in evented list
    /// </summary>
    public enum ChangeType
    {
      Add,
      Remove,
      Set,
      Clear
    }


    public delegate void ChangeHandler(EventedDictionary<TKey, TValue, TContext> dictionary,
                                        ChangeType change,
                                        EventPhase phase,
                                        TKey key,
                                        TValue value);

    #endregion


    #region .ctor

    ///<summary>
    /// Initializes a new instance that is empty, has the default initial capacity, and uses the default
    /// equality comparer for the key type.
    /// </summary>
    public EventedDictionary() : base()
     => m_Dictionary = new Dictionary<TKey, TValue>();

    ///<summary>
    /// Initializes a new instance that is empty, has the default initial capacity, and uses the default
    /// equality comparer for the key type.
    /// </summary>
    public EventedDictionary(TContext context, bool contextReadOnly) : base(context, contextReadOnly)
     => m_Dictionary = new Dictionary<TKey, TValue>();

    /// <summary>
    /// Initializes a new instance that contains elements copied from the specified IDictionary[TKey,TValue]
    ///  and uses the default equality comparer for the key type.
    /// </summary>
    public EventedDictionary(TContext context, bool contextReadOnly, IDictionary<TKey, TValue> dictionary) : base(context, contextReadOnly)
     => m_Dictionary = new Dictionary<TKey, TValue>(dictionary);

    /// <summary>
    ///   Initializes a new instance that is empty, has the default initial capacity, and uses the specified
    ///     IEqualityComparer[T].
    /// </summary>
    public EventedDictionary(TContext context, bool contextReadOnly, IEqualityComparer<TKey> comparer) : base(context, contextReadOnly)
     => m_Dictionary = new Dictionary<TKey, TValue>(comparer);

    /// <summary>
    /// Initializes a new instance that is empty, has the specified initial capacity, and uses the default
    ///   equality comparer for the key type.
    /// </summary>
    public EventedDictionary(TContext context, bool contextReadOnly, int capacity) : base(context, contextReadOnly)
     => m_Dictionary = new Dictionary<TKey, TValue>(capacity);

    /// <summary>
    ///  Initializes a new instance that contains elements copied from the specified IDictionary[TKey,TValue]
    ///     and uses the specified IEqualityComparer[T].
    /// </summary>
    public EventedDictionary(TContext context, bool contextReadOnly, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(context, contextReadOnly)
     => m_Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);

    /// <summary>
    /// Initializes a new instance of the Dictionary[TKey,TValue]
    ///   class that is empty, has the specified initial capacity, and uses the specified IEqualityComparer[T]
    /// </summary>
    public EventedDictionary(TContext context, bool contextReadOnly, int capacity, IEqualityComparer<TKey> comparer) : base(context, contextReadOnly)
     => m_Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);


    #endregion

    #region Private Fields

    private Dictionary<TKey, TValue> m_Dictionary;

    #endregion

    #region Events

    [field: NonSerialized] public ChangeHandler ChangeEvent;

    #endregion

    #region IDictionary members

    public void Add(TKey key, TValue value)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Add, EventPhase.Before, key, value);
      m_Dictionary.Add(key, value);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Add, EventPhase.After, key, value);
    }

    public bool ContainsKey(TKey key) => m_Dictionary.ContainsKey(key);

    public ICollection<TKey> Keys => m_Dictionary.Keys;

    public bool Remove(TKey key)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Remove, EventPhase.Before, key, default(TValue));
      var result = m_Dictionary.Remove(key);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Remove, EventPhase.After, key, default(TValue));
      return result;
    }

    public bool TryGetValue(TKey key, out TValue value) => m_Dictionary.TryGetValue(key, out value);

    public ICollection<TValue> Values => m_Dictionary.Values;

    public TValue this[TKey key]
    {
      get
      {
        return m_Dictionary[key];
      }
      set
      {
        CheckReadOnly();
        if (ChangeEvent != null) ChangeEvent(this, ChangeType.Set, EventPhase.Before, key, value);
        m_Dictionary[key] = value;
        if (ChangeEvent != null) ChangeEvent(this, ChangeType.Set, EventPhase.After, key, value);
      }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Add, EventPhase.Before, item.Key, item.Value);
      ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).Add(item);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Add, EventPhase.After, item.Key, item.Value);
    }

    public void Clear()
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Clear, EventPhase.Before, default(TKey), default(TValue));
      m_Dictionary.Clear();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Clear, EventPhase.After, default(TKey), default(TValue));
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).CopyTo(array, arrayIndex);
    }

    public int Count => m_Dictionary.Count;

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      CheckReadOnly();
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Remove, EventPhase.Before, item.Key, item.Value);
      var result = ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).Remove(item);
      if (ChangeEvent != null) ChangeEvent(this, ChangeType.Remove, EventPhase.After, item.Key, item.Value);
      return result;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
      => m_Dictionary.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      => m_Dictionary.GetEnumerator();

    #endregion

  }
}
