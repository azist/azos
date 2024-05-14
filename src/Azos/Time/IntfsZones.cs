/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Time
{
  /// <summary>
  /// Manages mapping of time zones by name in IANA(used by Linux) or Windows Time Zone names.
  /// You can also add a custom (non-os/system) timezone with custom names and add custom config to each (e.g. extra attributes)
  /// </summary>
  public interface ITimeZoneManager
  {
    /// <summary>
    /// Returns <see cref="TimeZoneMapping"/> object by name or throws id not found
    /// </summary>
    TimeZoneMapping GetZoneMapping(string name);

    /// <summary>
    /// Returns <see cref="TimeZoneMapping"/> object by name or null if not found
    /// </summary>
    TimeZoneMapping TryGetZoneMapping(string name);
  }

  public interface ITimeZoneManagerImplementation : ITimeZoneManager, IModule
  {
  }

  /// <summary>
  /// Types of zone mapping: Custom, IANA or Windows
  /// </summary>
  public enum TimeZoneMappingType
  {
    Custom = 0,
    IANA = 1,
    Windows = 2
  }

  /// <summary>
  /// Provides a named mapping which attaches <see cref="TimeZoneInfo"/> object by name
  /// with optional config vector which can be used for business purposes (e.g. storing non-standard business zone-specific flags)
  /// </summary>
  public sealed class TimeZoneMapping : INamed
  {
    /// <summary>
    /// Provides abstraction for getting complex <see cref="TimeZoneInfo"/> instances with
    /// custom <see cref="TimeZoneInfo.AdjustmentRule"/> and <see cref="TimeZoneInfo.TransitionTime"/> settings.
    /// You can store the data in a custom format, or even external storage such as DB/file/dictionary/etc
    /// </summary>
    public abstract class InfoProvider
    {
      protected InfoProvider(IConfigSectionNode cfg) { }
      public abstract TimeZoneInfo CreateInfo(TimeZoneMapping mapping);
    }

    /// <summary>
    /// Creates a custom zone mapping of the specified name
    /// </summary>
    public TimeZoneMapping(string name, IConfigSectionNode cfg)
    {
      cfg.NonEmpty(nameof(cfg));
      m_Name = name.NonBlank(nameof(name));
      m_MappingType = TimeZoneMappingType.Custom;
      var ndata = cfg["data"];
      if (ndata.Exists)
      {
        var cc = new LaconicConfiguration();
        cc.CreateFromNode(ndata);
        m_Data = cc.Root;
      }

      var nprovider = cfg["provider"];
      if (nprovider.Exists)
      {
        var provider = FactoryUtils.Make<InfoProvider>(nprovider, args: new []{ nprovider});
        m_Info = provider.CreateInfo(this);
      }
      else
      {
        var utcOffset = cfg.Of("utc-offset").ValueAsTimeSpan(TimeSpan.Zero);
        var displayName =  cfg.ValOf("display-name");
        var stdName = cfg.ValOf("std-name");
        m_Info = TimeZoneInfo.CreateCustomTimeZone(m_Name, utcOffset, displayName, stdName);
      }
    }

    /// <summary>
    /// Creates a system mapping for IANA or Windows
    /// </summary>
    public TimeZoneMapping(string name, TimeZoneMappingType type, IConfigSectionNode data, TimeZoneInfo sysInfo)
    {
      m_Name = name.NonBlank(nameof(name));
      m_MappingType = type;

      if (data != null && data.Exists)
      {
        var cc = new LaconicConfiguration();
        cc.CreateFromNode(data);
        m_Data = cc.Root;
      }

      m_Info = sysInfo.NonNull(nameof(sysInfo));
    }

    private string m_Name;
    private TimeZoneMappingType m_MappingType;
    private IConfigSectionNode m_Data;
    private TimeZoneInfo m_Info;

    /// <summary>
    /// Unique name of the mapping such as IANA or Windows name
    /// </summary>
    public string Name => m_Name;

    /// <summary> The type of mapping: Custom | IANA | WINDOWS </summary>
    public TimeZoneMappingType MappingType => m_MappingType;

    /// <summary> True when mapping is done by IANA name, otherwise it is a Windows or Custom mapping </summary>
    public bool IsIana => m_MappingType == TimeZoneMappingType.IANA;

    /// <summary> Windows mapping type </summary>
    public bool IsWindows => m_MappingType == TimeZoneMappingType.Windows;

    /// <summary> Custom mapping type - not IANA or Windows </summary>
    public bool IsCustom => !IsIana && !IsWindows;

    /// <summary> System mapping type - either an IANA or Windows </summary>
    public bool IsSystem => IsIana || IsWindows;

    /// <summary> Provides arbitrary data attached to this mapping OR NULL if not custom data was present </summary>
    public IConfigSectionNode Data => m_Data;

    /// <summary> Returns an instance of <see cref="TimeZoneInfo"/> which describes this zone</summary>
    public TimeZoneInfo ZoneInfo => m_Info;
  }

}
