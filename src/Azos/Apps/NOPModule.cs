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
    private static NOPModule s_Instance = new NOPModule();

    private NOPModule() : base(){ }

    /// <summary>
    /// Returns a singleton instance of the NOPModule
    /// </summary>
    public static NOPModule Instance
    {
      get { return s_Instance; }
    }

    public override bool IsHardcodedModule => true;

    protected override void DoConfigureChildModules(Conf.IConfigSectionNode node)
    {
    }
  }
}
