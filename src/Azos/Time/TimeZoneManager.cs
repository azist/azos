using System;
using System.Collections.Generic;
using System.Linq;
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

    public const string CONFIG_IANA_ATTR = "iana";
    public const string CONFIG_WIN_ATTR = "win";
    public const string CONFIG_NAMES_ATTR = "names";


    public TimeZoneManager(IModule parent) : base(parent)
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
        var iana = nmap.ValOf(CONFIG_IANA_ATTR);
        var win = nmap.ValOf(CONFIG_WIN_ATTR);
        var data = nmap[TimeZoneMapping.CONFIG_DATA_SECT];

        TimeZoneInfo info = null;

        if (iana.IsNotNullOrWhiteSpace())
          info = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(one => one.Id.EqualsIgnoreCase(iana));
        if (info == null && win.IsNotNullOrWhiteSpace())
          info = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(one => one.Id.EqualsIgnoreCase(win));

        info.NonNull($"System time zone `{iana.Default(win)}`");

        if (data != null && data.Exists)
        {
          var cc = new MemoryConfiguration();
          cc.CreateFromNode(data);
          data = cc.Root;
        }

        if (iana.IsNotNullOrWhiteSpace())
          m_Mappings.Register(new TimeZoneMapping(iana, TimeZoneMappingType.IANA, data, info)).IsTrue($"Unique IANA id '{iana}'");

        if (win.IsNotNullOrWhiteSpace())
          m_Mappings.Register(new TimeZoneMapping(win, TimeZoneMappingType.Windows, data, info));//WINDOWS names are NOT unique by design!

        var names = nmap.ValOf(CONFIG_NAMES_ATTR)?.Split(',', ';');
        if (names != null)
        {
          foreach (var oneName in names.Where(n => n.IsNotNullOrWhiteSpace()))
            m_Mappings.Register(new TimeZoneMapping(oneName, TimeZoneMappingType.SysAlias, data, info)).IsTrue($"Unique sys alias '{oneName}'");
        }
      }

      foreach (var nzone in node.ChildrenNamed(CONFIG_ZONE_SECTION))
      {
        var names = nzone.ValOf(CONFIG_NAMES_ATTR)?.Split(',',';');
        names.NonNull($"Configured `${CONFIG_NAMES_ATTR}`");
        foreach(var oneName in names.Where(n => n.IsNotNullOrWhiteSpace()))
          m_Mappings.Register(new TimeZoneMapping(oneName, nzone)).IsTrue($"Unique id '{oneName}'");
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
