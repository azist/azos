/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Scripting.Dsl;


namespace Azos.Scripting.Packaging.Dsl
{
  /// <summary>
  /// Base step for packing
  /// </summary>
  public abstract class PackageStepBase : Step
  {
    protected PackageStepBase(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
    [Config] public string Label { get; set; }
    [Config] public string Condition { get; set; }
    [Config] public string Description { get; set; }

    [Config(Default = 2)] public int Verbosity { get; set; }

    protected PackageBuilder Builder
    {
      get
      {
        if (Label.IsNullOrWhiteSpace())
        {
          return (Runner.Result as PackageBuilder).NonNull("Runner.Result: PackageBuilder");
        }
        return PackageBuilder.Get(Label);
      }
    }
  }
}
