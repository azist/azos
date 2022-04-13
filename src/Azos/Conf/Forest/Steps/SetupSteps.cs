/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;
using Azos.Scripting.Steps;
using Azos.Serialization.JSON;

namespace Azos.Conf.Forest.Steps
{

  public abstract class TreeStepBase : Step
  {
    public TreeStepBase(StepRunner runner, IConfigSectionNode cfg, int order)
      : base(runner, cfg, order){ }

    /// <summary>
    /// Gets dynamic dependency installed by LoadModule
    /// </summary>
    public IForestSetupLogic Logic => LoadModule.Get<IForestSetupLogic>();
  }


  /// <summary>
  /// Saves tree node
  /// </summary>
  public sealed class SaveNode : TreeStepBase
  {
    public SaveNode(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order) { }

    [Config]
    public string TreeNodeJson{ get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      var node = JsonReader.ToDoc<TreeNode>(TreeNodeJson.NonBlank(nameof(TreeNodeJson)));
      var got = Logic.SaveNodeAsync(node).GetAwaiter().GetResult();
      Runner.SetResult(got);
      return null;
    }
  }

  /// <summary>
  /// Saves tree node
  /// </summary>
  public sealed class DeleteNode : TreeStepBase
  {
    public DeleteNode(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order) { }

    [Config]
    public string Id { get; set; }

    [Config]
    public DateTime? StartUtc { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      Id.NonBlank(nameof(Id));
      var got = Logic.DeleteNodeAsync(Id, StartUtc).GetAwaiter().GetResult();
      Runner.SetResult(got);
      return null;
    }
  }



}
