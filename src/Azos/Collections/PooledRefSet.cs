/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace Azos.Collections
{
  /// <summary>
  /// Provides a pooled HashSet(object) which can be used in tight blocks which need to be thread safe yet try to avoid allocation.
  /// The pool is implemented using interlocked operation which eases memory pressure on GC in tight operations.
  /// You can not allocate an instance, and must use PooledRefSet.Acquire() instead, paired with instance.ReleaseToPool().
  /// Once released, an instance becomes not-thread safe and therefore should not be used anymore.
  /// If there are not pooled instances, then  `Acquire`  allocates a new instance of the heap.
  /// Most often this class is used for its static family of NoRefCycles(...) methods which implement a tight sync-only thread-bound reference
  /// cycle suppression state machine.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Most often this class is used for its static family of NoRefCycles(...) methods which implement a tight sync-only thread-bound reference
  /// cycle suppression state machine. The machine is ambient - that is: you do not need to pass its state between calls (which may be transitively dependent).
  /// Ambient state machines are used to suppress infinite loops/stack faults caused by cycles while traversing object graphs on any given thread.
  /// </para>
  /// <para>
  /// You can check if a reference is already processed and skip it, for example this is used for complex graph DI and validation (sync-only by design).
  /// </para>
  /// <para>
  /// The state machine design is inherently sync-only/thread based for performance, as asynchronous call flows would not benefit from such optimization and
  /// need to carry their operation state via calls flows using method parameters
  /// </para>
  /// </remarks>
  public sealed class PooledRefSet : HashSet<object>
  {

    private static PooledRefSet s_Set1;
    private static PooledRefSet s_Set2;
    private static PooledRefSet s_Set3;
    private static PooledRefSet s_Set4;
    private static PooledRefSet s_Set5;

    /// <summary>
    /// Acquires an existing instance from pool, or allocates a new one.
    /// You must release the instance with a call to ReleaseToPool()
    /// </summary>
    public static PooledRefSet Acquire()
    {
      var instance =
         Interlocked.Exchange(ref s_Set1, null) ??
         Interlocked.Exchange(ref s_Set2, null) ??
         Interlocked.Exchange(ref s_Set3, null) ??
         Interlocked.Exchange(ref s_Set4, null) ??
         Interlocked.Exchange(ref s_Set5, null);

      if (instance == null) instance = new PooledRefSet();

      return instance;
    }

    private PooledRefSet() : base(ReferenceEqualityComparer<object>.Instance){ }

    /// <summary>
    /// Release the instance back to pool, so it can be reused by a subsequent caller
    /// </summary>
    public void ReleaseToPool()
    {
      Clear();
      Thread.MemoryBarrier();
      if (null == Interlocked.CompareExchange(ref s_Set1, this, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Set2, this, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Set3, this, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Set4, this, null)) return;
      Interlocked.CompareExchange(ref s_Set5, this, null);
    }

    [ThreadStatic]
    private static PooledRefSet ts_Flow; //implicit ambient reference to call flow state machine


    /* NOTE:
     * The copious code below is needed for performance not to allocate extra closure instances,
     * because the versions differ in their signatures which are needed not to create arg closures
     */


    /// <summary>
    /// Implements a sync-only thread-bound reference cycle suppression state machine
    /// </summary>
    /// <typeparam name="TSelf">Type of cycle detection subject</typeparam>
    /// <typeparam name="TResult">The type of body result</typeparam>
    /// <param name="name">Name of the state machine - used for error reporting</param>
    /// <param name="instance">The instance to be added to set once - cycle detection subject</param>
    /// <param name="body">Lambda body</param>
    /// <returns>Result of body</returns>
    public static TResult NoRefCycles<TSelf, TResult>(string name, TSelf instance, Func<TSelf, PooledRefSet, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var flow = ts_Flow;
      if (flow==null)
      {
        flow = PooledRefSet.Acquire();
        ts_Flow = flow;
      }
      if (!flow.Add(instance)) throw new AzosException(StringConsts.FLOW_NO_REF_CYCLES_VIOLATION_ERROR.Args(name));
      try
      {
        return body(instance, flow);
      }
      finally
      {
        flow.Remove(instance);
        if (flow.Count==0)
        {
          ts_Flow = null;
          flow.ReleaseToPool();
        }
      }
    }

    /// <summary>
    /// Implements a sync-only thread-bound reference cycle suppression state machine
    /// </summary>
    /// <typeparam name="TSelf">Type of cycle detection subject</typeparam>
    /// <typeparam name="TArg1">Type of first body argument</typeparam>
    /// <typeparam name="TResult">The type of body result</typeparam>
    /// <param name="name">Name of the state machine - used for error reporting</param>
    /// <param name="instance">The instance to be added to set once - cycle detection subject</param>
    /// <param name="arg1">First body arg</param>
    /// <param name="body">Lambda body</param>
    /// <returns>Result of body</returns>
    public static TResult NoRefCycles<TSelf, TArg1, TResult>(string name, TSelf instance, TArg1 arg1, Func<TSelf, PooledRefSet, TArg1, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var flow = ts_Flow;
      if (flow == null)
      {
        flow = PooledRefSet.Acquire();
        ts_Flow = flow;
      }
      if (!flow.Add(instance)) throw new AzosException(StringConsts.FLOW_NO_REF_CYCLES_VIOLATION_ERROR.Args(name));
      try
      {
        return body(instance, flow, arg1);
      }
      finally
      {
        flow.Remove(instance);
        if (flow.Count == 0)
        {
          ts_Flow = null;
          flow.ReleaseToPool();
        }
      }
    }

    /// <summary>
    /// Implements a sync-only thread-bound reference cycle suppression state machine
    /// </summary>
    /// <typeparam name="TSelf">Type of cycle detection subject</typeparam>
    /// <typeparam name="TArg1">Type of first body argument</typeparam>
    /// <typeparam name="TArg2">Type of second body argument</typeparam>
    /// <typeparam name="TResult">The type of body result</typeparam>
    /// <param name="name">Name of the state machine - used for error reporting</param>
    /// <param name="instance">The instance to be added to set once - cycle detection subject</param>
    /// <param name="arg1">First body arg</param>
    /// <param name="arg2">Second body arg</param>
    /// <param name="body">Lambda body</param>
    /// <returns>Result of body</returns>
    public static TResult NoRefCycles<TSelf, TArg1, TArg2, TResult>(string name, TSelf instance, TArg1 arg1, TArg2 arg2, Func<TSelf, PooledRefSet, TArg1, TArg2, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var flow = ts_Flow;
      if (flow == null)
      {
        flow = PooledRefSet.Acquire();
        ts_Flow = flow;
      }
      if (!flow.Add(instance)) throw new AzosException(StringConsts.FLOW_NO_REF_CYCLES_VIOLATION_ERROR.Args(name));
      try
      {
        return body(instance, flow, arg1, arg2);
      }
      finally
      {
        flow.Remove(instance);
        if (flow.Count == 0)
        {
          ts_Flow = null;
          flow.ReleaseToPool();
        }
      }
    }

    /// <summary>
    /// Implements a sync-only thread-bound reference cycle suppression state machine
    /// </summary>
    /// <typeparam name="TSelf">Type of cycle detection subject</typeparam>
    /// <typeparam name="TArg1">Type of first body argument</typeparam>
    /// <typeparam name="TArg2">Type of second body argument</typeparam>
    /// <typeparam name="TArg3">Type of third body argument</typeparam>
    /// <typeparam name="TResult">The type of body result</typeparam>
    /// <param name="name">Name of the state machine - used for error reporting</param>
    /// <param name="instance">The instance to be added to set once - cycle detection subject</param>
    /// <param name="arg1">First body arg</param>
    /// <param name="arg2">Second body arg</param>
    /// <param name="arg3">Third body arg</param>
    /// <param name="body">Lambda body</param>
    /// <returns>Result of body</returns>
    public static TResult NoRefCycles<TSelf, TArg1, TArg2, TArg3, TResult>(string name, TSelf instance, TArg1 arg1, TArg2 arg2, TArg3 arg3, Func<TSelf, PooledRefSet, TArg1, TArg2, TArg3, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var flow = ts_Flow;
      if (flow == null)
      {
        flow = PooledRefSet.Acquire();
        ts_Flow = flow;
      }
      if (!flow.Add(instance)) throw new AzosException(StringConsts.FLOW_NO_REF_CYCLES_VIOLATION_ERROR.Args(name));
      try
      {
        return body(instance, flow, arg1, arg2, arg3);
      }
      finally
      {
        flow.Remove(instance);
        if (flow.Count == 0)
        {
          ts_Flow = null;
          flow.ReleaseToPool();
        }
      }
    }


  }
}
