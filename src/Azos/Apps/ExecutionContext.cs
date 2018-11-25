/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Threading;

namespace Azos.Apps
{
  /// <summary>
  /// Infrastructure class that should never be used in business applications
  /// Provides access to execution context - that groups Application and Session objects.
  /// All objects may be either application-global or (logical)thread-level.
  /// Effectively ExecutionContext.Application is the central chassis per process.
  /// The async code flows Session context automatically via Thread.Principal, however custom contexts should flow via passing it to functors.
  /// </summary>
  public static class ExecutionContext
  {
    private static volatile IApplication s_Application;
    private static Stack<IApplication> s_AppStack = new Stack<IApplication>();

    /// <summary>
    /// Returns global application context
    /// </summary>
    public static IApplication Application
    {
      get { return s_Application ?? NOPApplication.Instance; }
    }

    /// <summary>
    /// Returns Session object for current thread or async flow context, or if it is null NOPSession object is returned.
    /// Note: Thread.CurrentPrincipal auto-flows by async/await and other TAP
    /// </summary>
    public static ISession Session
    {
      get
      {
        var threadSession = Thread.CurrentPrincipal as ISession;
        return threadSession ?? NOPSession.Instance;
      }
    }

    /// <summary>
    /// Returns true when thread-level or async call context session object is available and not a NOPSession instance
    /// Note: Thread.CurrentPrincipal auto-flows by async/await and other TAP
    /// </summary>
    public static bool HasThreadContextSession
    {
      get
      {
        var threadSession = Thread.CurrentPrincipal as ISession;
        return threadSession != null  &&  threadSession.GetType() != typeof(NOPSession);
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
    /// Internal framework-only method to bind thread-level/async flow context
    /// </summary>
    public static void __SetThreadLevelSessionContext(ISession session)
    {
      Thread.CurrentPrincipal = session;
    }
  }
}
