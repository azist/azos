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
  /// Provides access to execution context - that groups Application, Request, Response and Session objects.
  /// All objects may be either application-global or thread-level.
  /// Effectively ExecutionContext.Application is the central DI/service locator facility per process.
  /// The async code should flow the context by passing it to functors.
  /// </summary>
  /// <remarks>
  /// This pattern is used on purpose based on careful evaluation of various DI frameworks use-cases in various projects,
  /// both server and client-side. The central service/locator hub per process as facilitated by the IApplication is the most intuitive and simple
  /// dependency resolution facility for 90+% of various business applications.
  /// </remarks>
  public static class ExecutionContext
  {
    private static volatile IApplication s_Application;
    private static volatile ISession s_Session;
    private static Stack<IApplication> s_AppStack = new Stack<IApplication>();

    /// <summary>
    /// Returns global application context
    /// </summary>
    public static IApplication Application
    {
      get { return s_Application ?? NOPApplication.Instance; }
    }

    /// <summary>
    /// Returns Session object for current thread or async flow context, or if it is null, app-global-level object is returned.
    /// Note: Thread.CurrentPrincipal auto-flows by async/await and other TAP
    /// </summary>
    public static ISession Session
    {
      get
      {
        var threadSession = Thread.CurrentPrincipal as ISession;
        return threadSession ?? s_Session ?? NOPSession.Instance;
      }
      //get { return ts_Session ?? s_Session ?? NOPSession.Instance; }
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
      //get { return ts_Session != null && ts_Session.GetType()!=typeof(NOPSession); }
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
      //ts_Session = session;
    }
  }
}
