using System;
using System.Collections.Generic;
using System.Text;
using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Time
{
  /// <summary>
  /// Provides default implementation of <see cref="ITimeZoneManager"/> module
  /// </summary>
  public sealed class TimeZoneManager : ModuleBase, ITimeZoneManagerImplementation
  {
    public const string CONFIG_ZONE_SECTION = "zone";
    public const string CONFIG_MAP_SECTION = "map";

    public TimeZoneManager(IApplication application) : base(application)
    {
      m_Mappings = new Registry<TimeZoneMapping>();
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.TIME_TOPIC;

    private Registry<TimeZoneMapping> m_Mappings;


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      m_Mappings.Clear();
      if (node==null || !node.Exists) return;

      foreach(var nmap in node.ChildrenNamed(CONFIG_MAP_SECTION))
      {

      }

      foreach (var nzone in node.ChildrenNamed(CONFIG_ZONE_SECTION))
      {

      }
    }

    #region ITimeZoneManager

    public TimeZoneMapping GetZoneMapping(string name)
    {
      throw new NotImplementedException();
    }

    public TimeZoneMapping TryGetZoneMapping(string name)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
