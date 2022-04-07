/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading;
using Azos.Collections;
using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Scripting.Steps
{
  /// <summary>
  /// Emits a log message
  /// </summary>
  public sealed class Log : Step
  {
    public Log(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx){ }

    [Config] public Azos.Log.MessageType MsgType{ get; set;}
    [Config] public string From { get; set; }
    [Config] public string Text { get; set; }
    [Config] public string Pars { get; set; }


    protected override string DoRun(JsonDataMap state)
    {
      WriteLog(MsgType, From, Text, null, null, Pars);
      return null;
    }
  }

  /// <summary>
  /// Emits a log message
  /// </summary>
  public sealed class See : Step
  {
    public See(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string Text { get; set; }
    [Config] public string Expression { get; set; }


    protected override string DoRun(JsonDataMap state)
    {
      Conout.See(Text);
      if (Expression.IsNotNullOrWhiteSpace())
      {
        var eval = new Text.Evaluator(Expression);
        var got = eval.Evaluate(id => Set.GetResolver(Runner, id, state));
        Conout.See(got);
      }

      return null;
    }
  }

  /// <summary>
  /// Runs a step with a delay in seconds
  /// </summary>
  public sealed class Delay : Step
  {
    public Delay(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public double Seconds { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      var secTimeout = Seconds;
      if (secTimeout <= 0.0) secTimeout = 1.0;

      var time = Timeter.StartNew();

      while (time.ElapsedSec < secTimeout && Runner.IsRunning)
        Thread.Sleep(100);

      return null;
    }
  }

}
