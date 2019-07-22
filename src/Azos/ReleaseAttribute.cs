using System;
using System.Collections.Generic;
using System.Text;

namespace Azos
{
  /// <summary>
  /// Provides categories for release information
  /// </summary>
  public enum ReleaseType
  {
    /// <summary> Unspecified release type</summary>
    Other = 0,

    /// <summary> Feature is in pre-release mode </summary>
    Preview,

    /// <summary> This is a release of a new version/feature </summary>
    Release,

    /// <summary> This release is focused on improvement/ providing additional/enhanced functionality</summary>
    Enhancement,

    /// <summary> This release is dedicated to patching/fixing issues</summary>
    Fix,

    /// <summary> The release is a planned scheduled maintenance/service </summary>
    Service
  }


  /// <summary>
  /// Provides code entities with custom metadata related to a release, grouping
  /// date, title and description as necessary
  /// </summary>
  [Release(ReleaseType.Release, 2019, 07, 23, "Initial release")]
  [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
  public sealed class ReleaseAttribute : Attribute  //todo:  Add custommetadataprovider
  {
    public ReleaseAttribute(ReleaseType type, int utcYear, int utcMonth, int utcDay, string title)
    {
      Type = type;
      ReleaseTimestampUtc = new DateTime(utcYear, utcMonth, utcDay, 0, 0, 0, DateTimeKind.Utc);
      Title = title.NonBlank(nameof(title));
    }

    public ReleaseAttribute(ReleaseType type, int utcYear, int utcMonth, int utcDay, int utcHour, int utcMinute, string title)
    {
      Type = type;
      ReleaseTimestampUtc = new DateTime(utcYear, utcMonth, utcDay, utcHour, utcMinute, 0, DateTimeKind.Utc);
      Title = title.NonBlank(nameof(title));
    }

    /// <summary> Release type: Enhancement/Fix/Service etc. </summary>
    public ReleaseType Type { get; }

    /// <summary> UTC Time stamp/ID of the release </summary>
    public DateTime ReleaseTimestampUtc{ get; }

    /// <summary> Provides release title </summary>
    public string Title { get; }

    /// <summary>
    /// Provides optional release version/tag number, this is typically used
    /// to correlate this feature release/change with software version tag, e.g. "2.0.5"
    /// </summary>
    public string Tag { get; set; }

    /// <summary> Provides optional release description </summary>
    public string Description { get; set; }

    /// <summary> Provides custom metadata in Laconic format </summary>
    public string Metadata { get; set; }
  }
}
