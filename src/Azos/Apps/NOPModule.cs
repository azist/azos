/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Apps
{
  /// <summary>
  /// Defines a module that does nothing
  /// </summary>
  public sealed class NOPModule : ModuleBase
  {
    public NOPModule(IApplication application) : base(application) { }

    public NOPModule(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => true;

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

    protected override void DoConfigureChildModules(Conf.IConfigSectionNode node)
    {
    }
  }
}
