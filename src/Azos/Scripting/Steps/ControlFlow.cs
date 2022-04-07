/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Steps
{
  /// <summary>
  /// Defines an entry point. Entry points need to be used as independently-addressable steps which execution can start from
  /// </summary>
  public class EntryPoint : Step
  {
    public EntryPoint(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
    protected override string DoRun(JsonDataMap state) => null;
  }

  /// <summary>
  /// Defines a subroutine which is a StepRunner tree
  /// </summary>
  public sealed class Sub : EntryPoint
  {
    public const string CONFIG_SOURCE_SECTION = "source";

    public Sub(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
      var nSource = cfg[CONFIG_SOURCE_SECTION].NonEmpty($"child section `/{CONFIG_SOURCE_SECTION}`");
      m_Body = FactoryUtils.Make<StepRunner>(nSource, typeof(StepRunner), new object[]{ runner.App, nSource, runner.GlobalState });
    }

    private StepRunner m_Body;

    protected override string DoRun(JsonDataMap state)
    {
      var local = m_Body.Run();
      state.Append(local, deep: true);
      return null;
    }
  }


  /// <summary>
  /// Transfers control to another step
  /// </summary>
  public sealed class Halt : Step
  {
    public Halt(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
    protected override string DoRun(JsonDataMap state) => throw new StepRunner.HaltSignal();
  }

  /// <summary>
  /// Transfers control to another step
  /// </summary>
  public sealed class Goto : Step
  {
    public Goto(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string Label { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      return Label;
    }
  }

  /// <summary>
  /// Calls a subroutine
  /// </summary>
  public sealed class Call : Step
  {
    public Call(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string Sub { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      Sub.NonBlank("call sub name");
      var inner = new StepRunner(App, Runner.RootSource, Runner.GlobalState);
      inner.Run(Sub);
      return null;
    }
  }

}
