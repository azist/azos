/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Platform;

namespace Azos.Apps
{
  /// <summary>
  /// Infrastructure class that should never be used in business applications
  /// Provides access to execution context - that groups Application and Session objects.
  /// All objects may be either application-global or (logical)thread-level.
  /// Effectively ExecutionContext.Application is the central chassis per process.
  /// The async code flows Session context automatically via AsyncFlowMutableLocal,
  /// however custom business contexts should flow via passing it to functors.
  /// </summary>
  public static class ExecutionContext
  {
    private static volatile IApplication s_Application;
    private static Stack<IApplication> s_AppStack = new Stack<IApplication>();
    private static AsyncFlowMutableLocal<ISession> ats_Session = new AsyncFlowMutableLocal<ISession>();
    private static AsyncFlowMutableLocal<ICallFlow> ats_CallFlow = new AsyncFlowMutableLocal<ICallFlow>();

    /// <summary>
    /// Returns global application context. The value is never null, the NOPApplication is returned
    /// had the specific app container not been allocated
    /// </summary>
    public static IApplication Application => s_Application ?? NOPApplication.Instance;

    /// <summary>
    /// Returns Session object for current call thread or async flow context, or if it is null NOPSession object is returned
    /// </summary>
    public static ISession Session
    {
      get
      {
        var callFlowSession = ats_Session.Value;
        return callFlowSession ?? NOPSession.Instance;
      }
    }

    /// <summary>
    /// Returns true when thread-level or async call context session object is available and not a NOPSession instance
    /// </summary>
    public static bool HasThreadContextSession
    {
      get
      {
        var callFlowSession = ats_Session.Value;
        return callFlowSession != null  && callFlowSession.GetType() != typeof(NOPSession);
      }
    }

    /// <summary>
    /// Returns a current call flow if any, or null if none is used
    /// </summary>
    public static ICallFlow CallFlow => ats_CallFlow.Value;

    /// <summary>
    /// Returns the effective ConsolePort - the one taken from the current App, if it is null app stack is tried.
    /// If non of the apps return ConsolePort, then NOPConsolePort is returned
    /// </summary>
    public static IO.Console.IConsolePort EffectiveApplicationConsolePort
    {
      get
      {
        var result = Application.ConsolePort;
        if (result!=null) return result;
        lock(s_AppStack)
        {
          foreach(var app in s_AppStack)//stack enumerates in reverse order of push()
          {
            result = app.ConsolePort;
            if (result!=null) return result;
          }
        }

        if (result==null) result = IO.Console.NOPConsolePort.Default;
        return result;
      }
    }

    /// <summary>
    /// Framework internal app bootstrapping method.
    /// Sets root application context
    /// </summary>
    public static void __BindApplication(IApplication application)
    {
      if (application==null || application is NOPApplication)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"__BindApplication(null|NOPApplication)");

      lock(s_AppStack)
      {
        if (s_AppStack.Contains(application))
          throw new AzosException(StringConsts.ARGUMENT_ERROR+"__BindApplication(duplicate)");

        if (s_Application!=null && !s_Application.AllowNesting)
          throw new AzosException(StringConsts.APP_CONTAINER_NESTING_ERROR.Args(application.GetType().FullName, s_Application.GetType().FullName));

        if (s_Application!=null)
          s_AppStack.Push( s_Application );

        s_Application = application;
      }
    }

    /// <summary>
    /// Framework internal app bootstrapping method.
    /// Resets root application context
    /// </summary>
    public static void __UnbindApplication(IApplication application)
    {
      lock(s_AppStack)
      {
        if (s_Application!=application)  return;
        if (s_AppStack.Count==0) return;

        s_Application = s_AppStack.Pop();
      }
    }

    /// <summary>
    /// Internal framework-only method to bind thread-level/async session context
    /// </summary>
    public static void __SetThreadLevelSessionContext(ISession session)
    {
      ats_Session.Value = session;
    }

    /// <summary>
    /// Internal framework-only method to bind thread-level/async flow context
    /// </summary>
    public static void __SetThreadLevelCallContext(ICallFlow call)
    {
      ats_CallFlow.Value = call;
    }

    /// <summary>
    /// System debug aid for advanced use - helps to identify classes
    /// which rely on CLR finalizers for their finalization which is a memory leak.
    /// Set to event handler which would get called on non-deterministic finalization.
    /// You can use ExecutionContext.__DefaultMemoryLeakTracker which logs the instance types.
    /// </summary>
    /// <remarks>
    /// The disposable objects MUST NOT rely on finalizers and must call .Dispose()
    /// deterministically.
    /// Warning: it is impossible to cache/use leaking object references for future use, as the call
    /// is being made from the finalizer, therefore the handler takes the object type only.
    /// Your custom tracker may keep track of types causing the leak, their frequency and dump the report
    /// at the end of app lifecycle
    /// </remarks>
    public static event Action<Type> __MemoryLeakTracker;

    /// <summary>
    /// System internal method which tracks non-deterministic object finalizations
    /// which indicate possible memory leaks
    /// </summary>
    public static void __TrackMemoryLeak(Type instanceType) => __MemoryLeakTracker?.Invoke(instanceType);

    /// <summary>
    /// Provides default memory leak tracking implementation based on app logging
    /// </summary>
    public static void __DefaultMemoryLeakTracker(Type instanceType)
    {
      if (instanceType==null) return;

      var app = Application;

      if (app is NOPApplication) return;

      var log = app.Log;

      if (log==null || log is Log.NOPLog) return;

      log.Write(new Log.Message
      {
        Type = Log.MessageType.Warning,
        Topic = CoreConsts.MEMORY_TOPIC,
        From = $"~{nameof(DisposableObject)}()",
        Text = StringConsts.OBJECT_WAS_NOT_DETERMINISTICALLY_DISPOSED_ERROR.Args(instanceType.FullName)
      });
    }
  }
}
