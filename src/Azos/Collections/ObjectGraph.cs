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
  /// Implements a tight sync-only thread-bound reference cycle suppression state machine, with internal pooled instance caching.
  /// Provides an efficient cycle detection mechanism which can be used in tight blocks which need to be thread safe yet try to avoid allocation.
  /// Internally a pooled state machine is implemented using interlocked operations which ease memory pressure on GC in tight operations.
  /// You can not allocate an instance, and must use ObjectGraph.Scope() method instead.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class is used for its static family of Scope(...) methods which implement a tight sync-only thread-bound reference
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
  public struct ObjectGraph
  {
    internal class state
    {
      private state()
      {
        m_Flow = new HashSet<object>(ReferenceEqualityComparer<object>.Instance);
        m_Visited = new HashSet<object>(ReferenceEqualityComparer<object>.Instance);
      }

      private static state s_Cache1;
      private static state s_Cache2;
      private static state s_Cache3;
      private static state s_Cache4;
      private static state s_Cache5;

      internal static state acquire()
      {
        var instance =
           Interlocked.Exchange(ref s_Cache1, null) ??
           Interlocked.Exchange(ref s_Cache2, null) ??
           Interlocked.Exchange(ref s_Cache3, null) ??
           Interlocked.Exchange(ref s_Cache4, null) ??
           Interlocked.Exchange(ref s_Cache5, null);

        if (instance == null) instance = new state();

        return instance;
      }


      //flow works as a stack: push, pull, consequently it does not capture cases like
      //for example traversing a linear list of objects which can repeat; flows capture cyclical references only
      internal readonly HashSet<object> m_Flow;
      //unlike flow, visited captures all visited references, even non-cyclical ones
      internal readonly HashSet<object> m_Visited;
      //call depth which controls scope lifetime. Scope gets destroyed on reaching zero
      internal int m_CallDepth;

      // Release the instance back to pool, so it can be reused by a subsequent caller
      internal void recycle()
      {
        m_CallDepth = 0;
        m_Flow.Clear();
        m_Visited.Clear();

        Thread.MemoryBarrier(); //<----------------------------------------------

        if (null == Interlocked.CompareExchange(ref s_Cache1, this, null)) return;
        if (null == Interlocked.CompareExchange(ref s_Cache2, this, null)) return;
        if (null == Interlocked.CompareExchange(ref s_Cache3, this, null)) return;
        if (null == Interlocked.CompareExchange(ref s_Cache4, this, null)) return;
        Interlocked.CompareExchange(ref s_Cache5, this, null);
      }

      internal bool add(object instance)
      {
        if (!m_Flow.Add(instance.NonNull(nameof(instance)))) return false;
        m_Visited.Add(instance);
        return true;
      }

      internal bool remove(object instance)
      {
        return m_Flow.Remove(instance.NonNull(nameof(instance)));
      }
    }

    private ObjectGraph(int depth, object current, state machine)
    {
      CallDepth = depth;
      Current = current;
      Machine = machine;
    }

    /// <summary>
    /// Depth of this call, 0 being an index for the root call that established this scope
    /// </summary>
    public readonly int CallDepth;

    /// <summary>
    /// Current instance being added for the first time, or null if this instance was visited before
    /// </summary>
    public readonly object Current;

    internal readonly state Machine;

    /// <summary>
    /// Returns true if the call flow already contains an instance. This is distinct from `Visited(instance)`, as call flow works as a stack
    /// while visited works as an add-only bag
    /// </summary>
    public bool InFlow(object instance) => Machine.m_Flow.Contains(instance);

    /// <summary>
    /// Returns true when the instance was visited before as part of graph processing under scope, and is not being added for the first time.
    /// `Visited` works as a bag where references get added, and never removed until scope destruction.
    /// This is distinct from `InFlow(instance)` which returns true only if the call flow already has a specified instance
    /// </summary>
    public bool Visited(object instance)
    {
      if (instance==null) return false;

      if (instance != Current) return Machine.m_Visited.Contains(instance);

      return false;
    }

    /// <summary>
    /// Adds an instance as visited, so it triggers Visited() check on next iterations
    /// </summary>
    public bool AddVisited(object instance)
    {
      if (instance==null) return false;
      return Machine.m_Visited.Add(instance);
    }

    [ThreadStatic]
    private static state ts_Machine; //implicit ambient reference to call flow state machine

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
    /// <returns>A tuple of (OK=TRUE, Result of body), otherwise OK=False when the call flow already contains the instance</returns>
    public static (bool OK, TResult result) Scope<TSelf, TResult>(string name, TSelf instance, Func<TSelf, ObjectGraph, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var state = ts_Machine;

      if (state==null)
      {
        state = state.acquire();
        ts_Machine = state;
      }

      var graph = new ObjectGraph(state.m_CallDepth,
                                  state.m_Visited.Contains(instance) ? null : instance,//check to see if it has not ALREADY been added before
                                  state);

      if (!state.add(instance)) return  (OK: false, result: default(TResult));

      state.m_CallDepth++;
      try
      {
        return (OK: true, result: body(instance, graph));
      }
      finally
      {
        state.m_CallDepth--;

        state.remove(instance);

        if (state.m_CallDepth <= 0)
        {
          ts_Machine = null;
          state.recycle();
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
    /// <returns>A tuple of (OK=TRUE, Result of body), otherwise OK=False when the call flow already contains the instance</returns>
    public static (bool OK, TResult result) Scope<TSelf, TArg1, TResult>(string name, TSelf instance, TArg1 arg1, Func<TSelf, ObjectGraph, TArg1, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var state = ts_Machine;

      if (state == null)
      {
        state = state.acquire();
        ts_Machine = state;
      }

      var graph = new ObjectGraph(state.m_CallDepth,
                                  state.m_Visited.Contains(instance) ? null : instance,//check to see if it has not ALREADY been added before
                                  state);

      if (!state.add(instance)) return (OK: false, result: default(TResult));

      state.m_CallDepth++;
      try
      {
        return (OK: true, result: body(instance, graph, arg1));
      }
      finally
      {
        state.m_CallDepth--;

        state.remove(instance);

        if (state.m_CallDepth <= 0)
        {
          ts_Machine = null;
          state.recycle();
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
    /// <returns>A tuple of (OK=TRUE, Result of body), otherwise OK=False when the call flow already contains the instance</returns>
    public static (bool OK, TResult result) Scope<TSelf, TArg1, TArg2, TResult>(string name, TSelf instance, TArg1 arg1, TArg2 arg2, Func<TSelf, ObjectGraph, TArg1, TArg2, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var state = ts_Machine;

      if (state == null)
      {
        state = state.acquire();
        ts_Machine = state;
      }

      var graph = new ObjectGraph(state.m_CallDepth,
                                  state.m_Visited.Contains(instance) ? null : instance,//check to see if it has not ALREADY been added before
                                  state);

      if (!state.add(instance)) return (OK: false, result: default(TResult));

      state.m_CallDepth++;
      try
      {
        return (OK: true, result: body(instance, graph, arg1, arg2));
      }
      finally
      {
        state.m_CallDepth--;

        state.remove(instance);

        if (state.m_CallDepth <= 0)
        {
          ts_Machine = null;
          state.recycle();
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
    /// <returns>A tuple of (OK=TRUE, Result of body), otherwise OK=False when the call flow already contains the instance</returns>
    public static (bool OK, TResult result) Scope<TSelf, TArg1, TArg2, TArg3, TResult>(string name, TSelf instance, TArg1 arg1, TArg2 arg2, TArg3 arg3, Func<TSelf, ObjectGraph, TArg1, TArg2, TArg3, TResult> body) where TSelf : class
    {
      instance.NonNull(nameof(instance));
      instance.NonNull(nameof(body));

      var state = ts_Machine;

      if (state == null)
      {
        state = state.acquire();
        ts_Machine = state;
      }

      var graph = new ObjectGraph(state.m_CallDepth,
                                  state.m_Visited.Contains(instance) ? null : instance,//check to see if it has not ALREADY been added before
                                  state);

      if (!state.add(instance)) return (OK: false, result: default(TResult));

      state.m_CallDepth++;
      try
      {
        return (OK: true, result: body(instance, graph, arg1, arg2, arg3));
      }
      finally
      {
        state.m_CallDepth--;

        state.remove(instance);

        if (state.m_CallDepth <= 0)
        {
          ts_Machine = null;
          state.recycle();
        }
      }
    }

  }
}
