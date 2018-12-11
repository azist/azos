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
  /// Provides access to process-global ambient context. Business app developers normally should not use this class
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


    /// <summary>
    /// Returns the memory utilization model for the application.
    /// This property is NOT configurable. It may be set at process entry point via a call to
    /// App.SetMemoryModel() before the app container spawns.
    /// Typical applications should not change the defaults.
    /// Some system service providers examine this property to allocate less cache and temp buffers
    /// in the memory-constrained environments
    /// </summary>
    public static MemoryUtilizationModel MemoryModel => s_MemoryModel;

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

      if (app!=null && !(app is NOPApplication))
        throw new AzosException(StringConsts.APP_SET_MEMORY_MODEL_ERROR);

      s_MemoryModel = model;
    }

    /// <summary>
    /// Shortcut access to  ExecutionContext.Application.TimeSource.UTCNow;
    /// </summary>
    public static DateTime UTCNow =>  ExecutionContext.Application.TimeSource.UTCNow;

    /// <summary>
    /// Returns process-wide random generator instance
    /// </summary>
    public static Platform.RandomGenerator Random => Platform.RandomGenerator.Instance;

    /// <summary>
    /// Returns the current call context user. The returned value is never null and returns fake user
    /// </summary>
    public static Security.User CurrentCallUser => ExecutionContext.Session.User;

  }
}
