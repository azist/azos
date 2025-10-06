/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Apps;

namespace Azos
{
  /// <summary>
  /// Provides access to ambient context such as CurrentCallSession/User information.
  /// The ambient context flows through async call chains.
  /// This class is mostly used for system components and frameworks that need ambient context - e.g. passing <see cref="ISession"/> and <see cref="ICallFlow"/>
  /// into inner functions which do not have these parameters in their signature.
  /// <br/>
  /// <br/>
  /// Typical business app code should NOT use this class directly, instead it should rely on DI (Dependency Injection).
  /// <br/>
  /// <br/>
  /// Another use case is legacy 3rd party apps where DI is not possible due to existing architecture.
  /// </summary>
  public static class Ambient
  {
    /// <summary>
    /// Denotes memory utilization modes
    /// </summary>
    public enum MemoryUtilizationModel
    {
      /// <summary>
      /// The application may use memory in a regular way without restraints. For example, a component
      /// may preallocate a few hundred megabyte lookup table on startup trading space for speed.
      /// </summary>
      Regular = 0,

      /// <summary>
      /// The application must try to use memory sparingly and not allocate large cache and buffers.
      /// This mode is typically used in a constrained 32bit apps and smaller servers. This mode gives
      /// a hint to components not to preallocate too much (e.g. do not preload 100 mb ZIP code database on startup)
      /// on startup. Trades performance for lower memory consumption
      /// </summary>
      Compact = -1,

      /// <summary>
      /// The application must try not to use extra memory for caches and temp buffers.
      /// This mode is typically used in a constrained 32bit apps and smaller servers.
      /// This mode is stricter than Compact
      /// </summary>
      Tiny = -2
    }


    private static MemoryUtilizationModel s_MemoryModel;
    private static string s_ProcessName;


    /// <summary>
    /// Returns the memory utilization model for the application.
    /// This property is NOT configurable. It may be set at process entry point via a call to
    /// Ambient.SetMemoryModel() before the app container spawns.
    /// Typical applications should not change the defaults.
    /// Some system service providers examine this property to allocate less cache and temp buffers
    /// in the memory-constrained environments
    /// </summary>
    public static MemoryUtilizationModel MemoryModel => s_MemoryModel;

    /// <summary>
    /// When set, return the logical name of this process which may be different than
    /// entry-point executable. For example `sky x` driver command sets `x` as a process name
    /// which is starts.
    /// Application container reads its configuration from process-name if it is set.
    /// If this value is not set then null is returned.
    /// This property is NOT configurable. It may be set at process entry point via a call to
    /// Ambient.SetProcessName() BEFORE the app container spawns.
    /// </summary>
    public static string ProcessName => s_ProcessName;

    /// <summary>
    /// Sets the memory utilization model for the whole app.
    /// This setting is NOT configurable. It may be set at process entry-point via a call to
    /// App.SetMemoryModel() before the app container spawns.
    /// Typical applications should not change the defaults.
    /// Some system service providers may examine this property to allocate less cache and temp buffers
    /// in the memory-constrained environments
    /// </summary>
    public static void SetMemoryModel(MemoryUtilizationModel model)
    {
      var app = ExecutionContext.Application;

      if (app != null && !(app is NOPApplication))
        throw new AzosException(StringConsts.APP_SET_MEMORY_MODEL_ERROR);

      s_MemoryModel = model;
    }

    /// <summary>
    /// Sets the logical process name - the entry point.
    /// This may be different from exe file.
    /// For example, an `sky x` driver program sets `x` as a process name.
    /// This setting is NOT configurable. It may be set at process entry-point via a call to
    /// App.SetProcessName() before the app container spawns
    /// </summary>
    public static void SetProcessName(string name)
    {
      var app = ExecutionContext.Application;

      if (app != null && !(app is NOPApplication))
        throw new AzosException(StringConsts.APP_SET_PROCESS_NAME_ERROR);

      s_ProcessName = name;
    }

    /// <summary>
    /// Shortcut access to  ExecutionContext.Application.ShutdownToken.
    /// You MUST store the returned token in your own variable at your scope entrance and NOT re-query the token because app context may change.
    /// This method should only be used in LEGACY 3rd PARTY APP where DI is not possible!!!
    /// <br/><br/>
    /// WARNING: LEGACY 3rd PARTY APP USE only: using this property in business code is a bad practice because it creates
    /// a hard dependency on the most current application chassis instance AT THE TIME of obtaining the token.
    /// This property is provided ONLY for legacy 3rd party apps which do not use DI and have no other way of getting notified
    /// of global application object shutdown.
    /// <br/>
    /// Any new project development should rely on DI and IApplication service injected into your code instead of this static property
    /// <see cref="IApplication.ShutdownToken"/>"/>
    /// </summary>
    [Obsolete("WARNING: LEGACY 3rd PARTY APP USE only: using this property in business code is a bad practice because " +
              "it creates a hard dependency on the most current application chassis instance AT THE TIME of obtaining the token." +
              "Any new project development should rely on DI and IApplication service injected into your code instead of this static property")]
    public static System.Threading.CancellationToken LegacyApplicationShutdownToken => ExecutionContext.Application.ShutdownToken;

    /// <summary>
    /// Shortcut access to  ExecutionContext.Application.TimeSource.UTCNow;
    /// </summary>
    public static DateTime UTCNow =>  ExecutionContext.Application.TimeSource.UTCNow;

    /// <summary>
    /// Returns process-wide random generator instance
    /// </summary>
    public static Platform.RandomGenerator Random => Platform.RandomGenerator.Instance;

    /// <summary>
    /// Returns an effective ConsolePort which is taken from the application/nesting chain
    /// </summary>
    public static IO.Console.IConsolePort AppConsolePort => ExecutionContext.EffectiveApplicationConsolePort;

    /// <summary>
    /// Returns the current call context user session. The returned value is never null and returns NOPSession with fake user if there is no real user session
    /// impersonating this call flow
    /// </summary>
    public static ISession CurrentCallSession => ExecutionContext.Session;

    /// <summary>
    /// Returns the current call context user. The returned value is never null and returns fake user if there is no real user injected impersonating this call flow
    /// </summary>
    public static Security.User CurrentCallUser => CurrentCallSession.User;

    /// <summary>
    /// Returns the current call flow or null
    /// </summary>
    public static ICallFlow CurrentCallFlow => ExecutionContext.CallFlow;

  }
}
