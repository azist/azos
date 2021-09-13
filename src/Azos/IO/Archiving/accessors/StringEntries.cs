/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Time;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Reads archives of String entries. The implementation is thread-safe
  /// </summary>
  [ContentTypeSupport(StringArchiveAppender.CONTENT_TYPE_PATTERN_ANY_STRING)]
  public sealed class StringArchiveReader : ArchiveBixReader<string>
  {
    public StringArchiveReader(IVolume volume) : base(volume){ }
    public override string MaterializeBix(BixReader reader) => reader.ReadString();
  }

  /// <summary>
  /// Reads archives of Json entries. The implementation is thread-safe
  /// </summary>
  [ContentTypeSupport(JsonArchiveAppender.CONTENT_TYPE_PATTERN_ANY_JSON)]
  public sealed class JsonArchiveReader : ArchiveBixReader<IJsonDataObject>
  {
    public JsonArchiveReader(IVolume volume) : base(volume) { }
    public override IJsonDataObject MaterializeBix(BixReader reader)
    {
      var json = reader.ReadString();
      if (json==null) return null;
      return JsonReader.DeserializeDataObject(json, true);
    }
  }

  /// <summary>
  /// Appends string entries to archive. The instance is NOT thread-safe
  /// </summary>
  [ContentTypeSupport(StringArchiveAppender.CONTENT_TYPE_STRING)]
  public sealed class StringArchiveAppender : ArchiveBixAppender<string>
  {
    public const string CONTENT_TYPE_STRING = "bix/string";
    public const string CONTENT_TYPE_PATTERN_ANY_STRING = "bix/string*";

    /// <summary>
    /// Appends string items to archive. The instance is NOT thread-safe
    /// </summary>
    public StringArchiveAppender(IVolume volume,
                                 ITimeSource time,
                                 Atom app,
                                 string host,
                                 Action<string, Bookmark> onPageCommit = null)
      : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, string entry)
      => wri.Write(entry);
  }

  /// <summary>
  /// Appends JSON object entries to archive. The instance is NOT thread-safe
  /// </summary>
  [ContentTypeSupport(JsonArchiveAppender.CONTENT_TYPE_JSON, StringArchiveAppender.CONTENT_TYPE_STRING)]
  public sealed class JsonArchiveAppender : ArchiveBixAppender<object>
  {
    public const string CONTENT_TYPE_JSON = "bix/string/json";
    public const string CONTENT_TYPE_PATTERN_ANY_JSON = "bix/string/json*";

    /// <summary>
    /// Appends string items to archive. The instance is NOT thread-safe
    /// </summary>
    public JsonArchiveAppender(IVolume volume,
                                 ITimeSource time,
                                 Atom app,
                                 string host,
                                 Action<object, Bookmark> onPageCommit = null)
      : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, object entry)
    {
      string json = entry==null ? null : JsonWriter.Write(entry, JsonWritingOptions.CompactRowsAsMap);
      wri.Write(json);
    }
  }
}
