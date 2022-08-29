/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Packaging.Dsl
{
  /// <summary>
  /// Sets target name
  /// </summary>
  public sealed class SetTarget : PackageStepBase
  {
    public SetTarget(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
    [Config] public string TargetName { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var cmd = new StartTargetCommand
      {
        Description = Eval(Description, state),
        TargetName = Eval(TargetName, state)
      };

      Builder.Appender.Append(cmd);


      return Task.FromResult<string>(null);
    }
  }
}
