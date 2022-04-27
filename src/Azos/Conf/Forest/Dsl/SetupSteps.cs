/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading.Tasks;
using Azos.Data;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Conf.Forest.Dsl
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

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var node = JsonReader.ToDoc<TreeNode>(Eval(TreeNodeJson.NonBlank(nameof(TreeNodeJson)), state).NonBlank(nameof(TreeNodeJson)));
      var got = await node.SaveAsync(App).ConfigureAwait(false);
      Runner.SetResult(got.GetResult());
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

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var id = Eval(Id.NonBlank(nameof(Id)), state).NonBlank(nameof(Id));
      var got = await Logic.DeleteNodeAsync(Id, StartUtc).ConfigureAwait(false);
      Runner.SetResult(got);
      return null;
    }
  }

}
