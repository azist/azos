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
  /// Emits a log message
  /// </summary>
  public sealed class Log : Step
  {
    public Log(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx){ }

    [Config] public Azos.Log.MessageType Type{ get; set;}
    [Config] public string From { get; set; }
    [Config] public string Text { get; set; }
    [Config] public string Pars { get; set; }


    protected override string DoRun(JsonDataMap state)
    {
      WriteLog(Type, From, Text, null, null, Pars);
      return null;
    }
  }

}
