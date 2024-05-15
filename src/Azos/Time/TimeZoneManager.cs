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
        var iana = nmap.ValOf("iana");
        var win = nmap.ValOf("win");
        var data = nmap["data"];

        TimeZoneInfo info = null;
        if (iana.IsNotNullOrWhiteSpace())
        {
          info = TimeZoneInfo.FindSystemTimeZoneById(iana);
          m_Mappings.Register(new TimeZoneMapping(iana, TimeZoneMappingType.IANA, data, info)).IsTrue($"Unique id '{iana}'");
        }

      }

      foreach (var nzone in node.ChildrenNamed(CONFIG_ZONE_SECTION))
      {

      }
    }

    #region ITimeZoneManager
    /// <inheritdoc/>
    public TimeZoneMapping GetZoneMapping(string name) => TryGetZoneMapping(name).NonNull($"Existing zone mapping '{name}'");

    /// <inheritdoc/>
    public TimeZoneMapping TryGetZoneMapping(string name) => m_Mappings[name.NonBlank(nameof(name))];

    #endregion
  }
}
