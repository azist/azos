/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  /// Throws an exception with a message if directory does not exist
  /// </summary>
  public sealed class DirExists : Step
  {
    public DirExists(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config]
    public string Path { get; set; }

    [Config]
    public string Msg { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var path = Eval(Path, state);

      if (!System.IO.Directory.Exists(path))
      {
        throw new RunnerException(Eval(Msg.Default("Directory does not exist: {0}".Args(path)), state));
      }

      return Task.FromResult<string>(null);
    }
  }

  /// <summary>
  /// Throws an exception with a message if file does not exist
  /// </summary>
  public sealed class FileExists : Step
  {
    public FileExists(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config]
    public string Path { get; set; }

    [Config]
    public string Msg { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var path = Eval(Path, state);

      if (!System.IO.File.Exists(path))
      {
        throw new RunnerException(Eval(Msg.Default("File does not exist: {0}".Args(path)), state));
      }

      return Task.FromResult<string>(null);
    }
  }

}
