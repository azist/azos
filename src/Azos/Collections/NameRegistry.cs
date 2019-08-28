/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Azos.Collections
{
    /// <summary>
    /// Denotes an entity that has a container-wide unique Name property.
    /// This interface is primarily used with Registry[INamed] class that allows for
    ///  string-based addressing (getting instances by object instance name).
    /// The names are used by many system functions, like addressing components by name
    /// in configuration and admin tools
    /// </summary>
    public interface INamed
    {
      string Name { get; }
    }

    /// <summary>
    /// Denotes an entity that has a relative Order property within a collection of entities
    /// </summary>
    public interface IOrdered
    {
      int Order { get; }
    }

    /// <summary>
    /// Provides read-only named object lookup capabilities
    /// </summary>
    public interface IRegistry<out T> : IEnumerable<T> where T : INamed
    {
      IEnumerable<string> Names { get; }
      IEnumerable<T> Values { get; }

      /// <summary>
      /// Returns item by name or default item (such as null) if the named instance could not be found
      /// </summary>
      T this[string name] { get;}

      /// <summary>
      /// Returns true if the instance differentiates names by case
      /// </summary>
      bool IsCaseSensitive{ get;}

      /// <summary>
      /// Returns true if when this registry contains the specified name
      /// </summary>
      bool ContainsName(string name);

      /// <summary>
      /// Returns the count of items registered in this instance
      /// </summary>
      int Count{ get;}
    }

    /// <summary>
    /// Provides read-only named ordered object lookup capabilities
    /// </summary>
    public interface IOrderedRegistry<out T> : IRegistry<T> where T : INamed, IOrdered
    {
      /// <summary>
      /// Returns items that registry contains ordered by their Order property.
      /// The returned sequence is pre-sorted during alteration of registry, so this property access is efficient.
      /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
      ///  of ordered items which may capture items that have already/not yet been added to the registry
      /// </summary>
      IEnumerable<T> OrderedValues{ get;}

      /// <summary>
      /// Tries to return an item by its position index in ordered set of items that this registry keeps.
      /// Null is returned when index is out of bounds.
      /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
      ///  of ordered items which may capture items that have already/not yet been added to the registry
      /// </summary>
      T this[int index] { get;}
    }



    /// <summary>
    /// Internal dictionary of string-named objects
    /// </summary>
    [Serializable]
    internal class RegistryDictionary<T> : Dictionary<string, T> where T : INamed
    {
       public RegistryDictionary(bool caseSensitive) : base(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
       {
       }

       public RegistryDictionary(bool caseSensitive, IDictionary<string, T> other) : base(other, caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
       {
       }

       protected RegistryDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
       {
       }
    }


    /// <summary>
    /// Represents a thread-safe registry of T. This class is efficient for lock-free concurrent read access and is
    /// not designed for cases when frequent modifications happen. It is ideal for lookup of named instances
    /// (such as components) that have much longer life time span than components that look them up.
    /// Registry performs lock-free lookup which speeds-up many concurrent operations that need to map
    /// names into objects. The enumeration over registry makes a snapshot of its data, hence a registry may
    /// get modified by other threads while being enumerated (snapshot consistency).
    /// </summary>
    [Serializable]
    public class Registry<T> : IRegistry<T> where T : INamed
    {
      public Registry() : this(false)
      {
      }

      public Registry(bool caseSensitive)
      {
        m_CaseSensitive = caseSensitive;
        m_Data = new RegistryDictionary<T>(caseSensitive);

        Thread.MemoryBarrier();
      }

      public Registry(IEnumerable<T> other) : this(other, false)
      {
      }

      public Registry(IEnumerable<T> other, bool caseSensitive) : this(caseSensitive)
      {
        foreach(var item in other)
          m_Data[item.Name] = item;

        Thread.MemoryBarrier();
      }


      [NonSerialized]
      protected object m_Sync = new object();

      private bool m_CaseSensitive;
      private volatile RegistryDictionary<T> m_Data;


      /// <summary>
      /// Returns true if the instance differentiates names by case
      /// </summary>
      public bool IsCaseSensitive => m_CaseSensitive;

      /// <summary>
      /// Returns a value by name or null if not found
      /// </summary>
      public T this[string name]
      {
        get
        {
          var data = m_Data;//atomic
          T result;
          if (data.TryGetValue(name, out result)) return result;
          return default(T);
        }
      }

      /// <summary>
      /// Returns the number of entries in the registry
      /// </summary>
      public int Count => m_Data.Count;

      /// <summary>
      /// Registers item and returns true if it was registered, false if this named instance already existed in the list
      /// </summary>
      public bool Register(T item)
      {
        lock(m_Sync)
        {
          if (m_Data.ContainsKey(item.Name)) return false;

          var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);
          data.Add(item.Name, item);

          JustRegistered(item);

          Thread.MemoryBarrier();
          m_Data = data; //atomic
        }

        return true;
      }

      /// <summary>
      /// Registers item and returns true if it was registered, false if this named instance already existed and was replaced
      /// </summary>
      public bool RegisterOrReplace(T item)
      {
        T existing;
        return RegisterOrReplace(item, out existing);
      }

      /// <summary>
      /// Registers item and returns true if it was registered, false if this named instance already existed and was replaced
      /// </summary>
      public bool RegisterOrReplace(T item, out T existing)
      {
        bool hadExisting;
        lock(m_Sync)
        {
          var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);

          if (data.TryGetValue(item.Name, out existing))
          {
              hadExisting = true;
              data[item.Name] = item;
              JustReplaced(existing, item);
          }
          else
          {
              hadExisting = false;
              existing = default(T);//safeguard
              data.Add(item.Name, item);
              JustRegistered(item);
          }

          Thread.MemoryBarrier();
          m_Data = data; //atomic
        }

        return !hadExisting;
      }

      /// <summary>
      /// Unregisters item and returns true if it was unregistered, false if it did not exist
      /// </summary>
      public bool Unregister(T item)
      {
        lock(m_Sync)
        {
          if (!m_Data.ContainsKey(item.Name)) return false;

          var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);
          data.Remove(item.Name);

          JustUnregistered(item);

          Thread.MemoryBarrier();
          m_Data = data; //atomic
        }

        return true;
      }

      /// <summary>
      /// Unregisters item by name and returns true if it was unregistered, false if it did not exist
      /// </summary>
      public bool Unregister(string name)
      {
        lock(m_Sync)
        {
          T item;
          if (!m_Data.TryGetValue(name, out item)) return false;

          var data = new RegistryDictionary<T>(m_CaseSensitive, m_Data);
          data.Remove(name);

          JustUnregistered(item);

          Thread.MemoryBarrier();
          m_Data = data; //atomic
        }

        return true;
      }

      /// <summary>
      /// Deletes all items from registry
      /// </summary>
      public virtual void Clear()
      {
        lock(m_Sync)
        {
          m_Data = new RegistryDictionary<T>(m_CaseSensitive);
        }
      }


      /// <summary>
      /// Tries to find an item by name, and returns it if it is found, otherwise calls a factory function supplying context value and registers the obtained
      ///  new item. The first lookup is performed in a lock-free way and if an item is found then it is immediately returned.
      ///  The second check and factory call operation is performed atomically under the lock to ensure consistency
      /// </summary>
      public T GetOrRegister<TContext>(string name, Func<TContext, T> regFactory, TContext context)
      {
        bool wasAdded;
        return this.GetOrRegister<TContext>(name, regFactory, context, out wasAdded);
      }


      /// <summary>
      /// Tries to find an item by name, and returns it if it is found, otherwise calls a factory function supplying context value and registers the obtained
      ///  new item. The first lookup is performed in a lock-free way and if an item is found then it is immediately returned.
      ///  The second check and factory call operation is performed atomically under the lock to ensure consistency
      /// </summary>
      public T GetOrRegister<TContext>(string name, Func<TContext, T> regFactory, TContext context, out bool wasAdded)
      {
        //1st check - lock-free lookup attempt
        var data = m_Data;
        T result;
        if (data.TryGetValue(name, out result))
        {
          wasAdded = false;
          return result;
        }

        lock(m_Sync)
        {
          //2nd check under lock
          data = m_Data;//must re-read the reference
          if (data.TryGetValue(name, out result))
          {
            wasAdded = false;
            return result;
          }
          result = regFactory( context );
          Register( result );
          wasAdded = true;
          return result;
        }
      }

      public IEnumerator<T> GetEnumerator()
      {
        var data = m_Data;//atomic
        return data.Values.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

      public IEnumerable<string> Names
      {
        get
        {
          var data = m_Data;
          return data.Keys;
        }
      }

      public IEnumerable<T> Values
      {
        get
        {
          var data = m_Data;
          return data.Values;
        }
      }

      public bool ContainsName(string name)
      {
        var data = m_Data;
        return data.ContainsKey(name);
      }

      public bool TryGetValue(string name, out T value)
      {
        var data = m_Data;
        return data.TryGetValue(name, out value);
      }

      //called under the lock
      protected virtual void JustRegistered(T item) {}

      //called under the lock
      protected virtual void JustReplaced(T existingItem, T newItem) {}

      //called under the lock
      protected virtual void JustUnregistered(T item) {}
    }


    /// <summary>
    /// Represents a thread-safe registry of T which is ordered by item's Order property.
    /// This class is efficient for concurrent read access and is not designed for cases when frequent modifications happen.
    /// It is ideal for lookup of named instances that have much longer time span than components that look them up.
    /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
    ///  of ordered items which may capture items that have already/not yet been added to the registry
    /// </summary>
    [Serializable]
    public class OrderedRegistry<T> : Registry<T>, IOrderedRegistry<T> where T : INamed, IOrdered
    {
      public OrderedRegistry() : this(false)
      {
      }

      public OrderedRegistry(bool caseSensitive) : base(caseSensitive)
      {
        m_OrderedValues = new List<T>();

        Thread.MemoryBarrier();
      }


      private List<T> m_OrderedValues;

      /// <summary>
      /// Returns items that registry contains ordered by their Order property.
      /// The returned sequence is pre-sorted during alteration of registry, so this property access is efficient.
      /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
      ///  of ordered items which may capture items that have already/not yet been added to the registry
      /// </summary>
      public IEnumerable<T> OrderedValues => m_OrderedValues;

      /// <summary>
      /// Tries to return an item by its position index in ordered set of items that this registry keeps.
      /// Null is returned when index is out of bounds.
      /// Note: since registry does reading in a lock-free manner, it is possible to have an inconsistent read snapshot
      ///  of ordered items which may capture items that have already/not yet been added to the registry
      /// </summary>
      public T this[int index]
      {
        get
        {
          var lst = m_OrderedValues;//thread-safe copy
          if (index>=0 && index<lst.Count)
              return lst[index];

          return default(T);
        }
      }

        /// <summary>
      /// Deletes all items from ordered registry
      /// </summary>
      public override void Clear()
      {
        lock(m_Sync)
        {
          base.Clear();
          m_OrderedValues = new List<T>();
        }
      }

      //called under the lock
      protected override void JustRegistered(T item)
      {
        var list = new List<T>(m_OrderedValues);
        list.Add(item);
        list.Sort( (l, r) => l.Order.CompareTo(r.Order) );

        Thread.MemoryBarrier();
        m_OrderedValues = list;
      }

      //called under the lock
      protected override void JustReplaced(T existingItem, T newItem)
      {
        var list = new List<T>(m_OrderedValues);
        list.Remove(existingItem);
        list.Add(newItem);
        list.Sort( (l, r) => l.Order.CompareTo(r.Order) );

        Thread.MemoryBarrier();
        m_OrderedValues = list;
      }

      //called under the lock
      protected override void JustUnregistered(T item)
      {
        var list = new List<T>(m_OrderedValues);
        list.Remove(item);

        Thread.MemoryBarrier();
        m_OrderedValues = list;
      }
   }

}
