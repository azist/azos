/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  /// Builds object from an instruction node
  /// </summary>
  /// <example><code>
  /// structure
  /// {
  ///   a=1
  ///   b=2
  ///   c{ z="ok" }
  ///   [d]=10
  ///   [d]=-22
  ///   [d]=true
  ///   [d]{ ok=false}
  /// }
  /// =>
  /// { "a": 1, "b": 2, "c": {"z": "ok"}, d: [ 10,-22,true, {"ok": false}]}
  /// </code></example>
  public sealed class SetObject : Step
  {
    public const string CONFIG_STRUCTURE_SECT = "structure";

    public SetObject(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx){ }

    [Config] public string From   { get; set; }
    [Config] public string Global { get; set; }
    [Config] public string Local  { get; set; }
    [Config(CONFIG_STRUCTURE_SECT)] public IConfigSectionNode Structure { get; set; }


    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var from = Eval(From, state);
      JsonDataMap obj = null;
      if (from.IsNotNullOrWhiteSpace())
      {
        obj = from.JsonToDataObject() as JsonDataMap;
      }

      if (obj == null) obj = new JsonDataMap(true);

      Structure.BuildJsonObjectFromConfigSnippet(obj, (v) => Eval(v.AsString(), state) );

      if (Global.IsNotNullOrWhiteSpace())
      {
        Runner.GlobalState[Global] = obj;
      }

      if (Local.IsNotNullOrWhiteSpace())
      {
        state[Local] = obj;
      }

      return Task.FromResult<string>(null);
    }

  }

}
