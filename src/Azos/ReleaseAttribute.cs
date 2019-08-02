using System;
using System.Reflection;

using Azos.Conf;

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
  /// Decorates code entities (e.g. classes, methods) with custom metadata related to a release, grouping
  /// date, title and description as necessary
  /// </summary>
  [Release(ReleaseType.Release, 2019, 07, 23, "Initial release", Tags="api-doc new")]
  [CustomMetadata(typeof(ReleaseCustomMetadataProvider))]
  [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
  public sealed class ReleaseAttribute : Attribute
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
    /// Provides optional release version/tags, this is typically used
    /// to correlate this feature release/change with software version tag, e.g. "2.0.5"
    /// Tags are separates by spaces by convention
    /// </summary>
    public string Tags { get; set; }

    /// <summary> Provides optional release description </summary>
    public string Description { get; set; }

    /// <summary> Provides custom metadata in Laconic format </summary>
    public string Metadata { get; set; }
  }

  /// <summary>
  /// Described releases as set by ReleaseAttribute
  /// </summary>
  public sealed class ReleaseCustomMetadataProvider : CustomMetadataProvider
  {
    public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
    {
      if (instance is ReleaseAttribute release)
      {
        var node = dataRoot.AddChildNode("release");
        node.AddAttributeNode("type", release.Type);
        node.AddAttributeNode("utc", release.ReleaseTimestampUtc);
        node.AddAttributeNode("title", release.Title);

        if (release.Tags.IsNotNullOrWhiteSpace())
          node.AddAttributeNode("tags", release.Tags);

        if (release.Description.IsNotNullOrWhiteSpace())
          node.AddAttributeNode("descr", release.Description);

        if (context.DetailLevel > MetadataDetailLevel.Public)
        {
          if (release.Metadata.IsNotNullOrWhiteSpace())
            node.AddAttributeNode("meta", release.Metadata);
        }
      }

      return dataRoot;
    }
  }
}
