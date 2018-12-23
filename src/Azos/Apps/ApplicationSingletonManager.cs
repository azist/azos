using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Apps
{
  /// <summary>
  /// Provides singleton instance management (get/set/remove) functionality.
  /// The implementor gets allocated once per application context.  All methods are thread-safe.
  /// Singleton instances are keyed on their type - this is designed on purpose as components should not
  /// create global ad hoc object instances (see remarks). The manager disposes all singleton instances on
  /// its own dispose, to bypass this behavior remove all items yourself prior to disposing ApplicationSingletonManager itself
  /// </summary>
  /// <remarks>
  /// Why singletons keyed on type vs. app-global instances keyed on arbitrary value? -
  /// this is done on purpose as we do not want to promote global instance creation.
  /// Should you need to create global instances do so via specially-designed root entity such as module
  /// or app-wide singleton.
  /// </remarks>
  public interface IApplicationSingletonManager : IEnumerable<object>
  {
    /// <summary>
    /// Tries to get a singleton instance or null if it does not exist
    /// </summary>
    T Get<T>() where T : class;

    /// <summary>
    /// Tries to get a singleton instance if it exists, if does not then calls a factory and
    /// sets under thread-safe lock. Returns a tuple of (T, bool) later set to true if factory was invoked.
    /// If factory call returns null, the whole Create is canceled (as-if only Get() was called)
    /// </summary>
    (T instance, bool created) GetOrCreate<T>(System.Func<T> factory) where T : class;

    /// <summary>
    /// Tries to remove instance for the specified type. Returns true if found and removed.
    /// Does NOT call the .Dispose() on the IDisposable instances
    /// </summary>
    bool Remove<T>() where T : class;

    //Note: there is no Clear() method because we do not want to give component writers a way to remove
    //someone else's singletons
  }



  /// <summary>
  /// Implements a disposable IApplicationSingletonManager
  /// </summary>
  public sealed class ApplicationSingletonManager : DisposableObject, IApplicationSingletonManager
  {
    public ApplicationSingletonManager() { }

    protected override void Destructor()
    {
      this.ForEach( s => { if (s is IDisposable d) d.Dispose(); } );
    }


    private object m_Lock = new object();
    private volatile Dictionary<Type, object> m_Instances = new Dictionary<Type, object>();

    public T Get<T>() where T : class
    {
      var tp = typeof(T);

      if (m_Instances.TryGetValue(tp, out object v)) return (T)v;

      return null;
    }

    public (T instance, bool created) GetOrCreate<T>(Func<T> factory) where T : class
    {
      var tp = typeof(T);
      //1st lookup is lock-free
      if (m_Instances.TryGetValue(tp, out object existing1)) return ((T)existing1, false);

      lock (m_Lock)
      {
        //2nd lookup under the lock
        if (m_Instances.TryGetValue(tp, out object existing2)) return ((T)existing2, false);

        if (factory==null) return (null, false);

        T newInstance = factory();

        if (newInstance==null) return (null, false);

        var dict = new Dictionary<Type, object>(m_Instances);
        dict.Add(tp, newInstance);
        m_Instances = dict;//atomic

        return (newInstance, true);
      }
    }

    public bool Remove<T>() where T : class
    {
      var tp = typeof(T);
      lock (m_Lock)
      {
        //not to create extra copy
        if (!m_Instances.ContainsKey(tp)) return false;

        var dict = new Dictionary<Type, object>(m_Instances);
        dict.Remove(tp);
        m_Instances = dict;//atomic
      }

      return true;
    }

    public IEnumerator<object> GetEnumerator() => m_Instances.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Instances.Values.GetEnumerator();
  }


  /// <summary>
  /// Implements IApplicationSingletonManager by doing nothing
  /// </summary>
  public sealed class NOPApplicationSingletonManager : DisposableObject, IApplicationSingletonManager
  {
    public T Get<T>() where T : class => null;
    public (T instance, bool created) GetOrCreate<T>(Func<T> factory) where T : class => (null, false);
    public bool Remove<T>() where T : class => false;

    public IEnumerator<object> GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();
  }


}
