using System;
using System.IO;
using System.Reflection;

using Azos.Conf;

namespace Azos.Scripting
{
  /// <summary>
  /// Describes a target of runner
  /// </summary>
  public interface IRunnerHost : IConfigurable, IDisposable
  {

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

    TextWriter ConsoleOut{ get; }
    TextWriter ConsoleError{ get; }

    void Summarize(Runner runner);
  }
}
