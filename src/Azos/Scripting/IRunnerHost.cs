/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Reflection;

using Azos.Apps;
using Azos.Conf;
using Azos.IO.Console;

namespace Azos.Scripting
{
  /// <summary>
  /// Describes a target of runner
  /// </summary>
  public interface IRunnerHost : IApplicationComponent, IConfigurable, IDisposable
  {
    /// <summary>
    /// The console port used for console interaction such as printing messages
    /// </summary>
    IConsolePort ConsolePort { get; }

    /// <summary>
    /// Sets the output file name/type (via extension)
    /// </summary>
    string OutFileName { get; set; }

    int TotalRunnables { get; }
    int TotalMethods   { get; }
    int TotalOKs       { get; }
    int TotalErrors    { get; }

    void Start(Runner runner);

    void BeginRunnable(Runner runner, FID id, object runnable);
    void EndRunnable(Runner runner, FID id, object runnable, Exception error);

    void BeforeMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr);
    void AfterMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error);


    void Summarize(Runner runner);
  }
}
