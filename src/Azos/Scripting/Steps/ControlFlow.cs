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
  /// Transfers control to another step
  /// </summary>
  public sealed class Goto : Step
  {
    public Goto(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string GotoName { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      return GotoName;
    }
  }

}
