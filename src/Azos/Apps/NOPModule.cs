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
