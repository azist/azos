using System;
using System.Collections;
using System.IO;

using Azos.Collections;
using Azos.Serialization.JSON;

namespace Azos.Data.Directory
{
  public enum ItemStatus{ Created = 0, Updated, Deleted }

  /// <summary>
  /// Represents an item stored in a directory
  /// </summary>
  [Serializable]
  public sealed class Item : IJsonWritable, IJsonReadable
  {
    #region Props
    public ItemId Id {  get; internal set; }

    /// <summary>
    /// The UTC timestamp of this version
    /// </summary>
    public DateTime VersionUtc { get; internal set; }

    /// <summary>
    /// The status of this item: Created/Updated/Deleted
    /// </summary>
    public ItemStatus VersionStatus { get; internal set; }


    /// <summary>
    /// Optional future point in time when item expires (disappears). Expressed as UTC timestamp
    /// </summary>
    public DateTime? AbsoluteExpirationUtc {  get; internal set; }


    /// <summary>
    /// The UTC timestamp of the last use, such as Create/Update/Get(touch=true) or Touch(id). This is used for optional SlidingExpirationMinutes
    /// </summary>
    public DateTime LastUseUtc {  get; internal set; }

    /// <summary>
    /// If greater than zero, sets the sliding expiration life span. Works together with LastUseUtc
    /// </summary>
    public int SlidingExpirationMinutes {  get; internal set; }

    /// <summary>
    /// Item's data
    /// </summary>
    public string Data {  get; internal set; }

    /// <summary>
    /// Item index entries, a list of name=value pairs which will be indexed
    /// </summary>
    public StringMap Index {  get; internal set;}

    #endregion

    #region Json
    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options,
          new DictionaryEntry("id", this.Id),
          new DictionaryEntry("_v", this.VersionUtc),
          new DictionaryEntry("_s", this.VersionStatus),
          new DictionaryEntry("absutc", this.AbsoluteExpirationUtc),
          new DictionaryEntry("lutc", this.LastUseUtc),
          new DictionaryEntry("semin", this.SlidingExpirationMinutes),
          new DictionaryEntry("data", this.Data),
          new DictionaryEntry("idx", Index)
       );
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.NameBinding? nameBinding)
    {
      if (data == null) return (true, null);
      if (data is JsonDataMap map)
      {
        var (m, id) = ((IJsonReadable)Id).ReadAsJson(map["id"], fromUI, nameBinding);
        if (!m) return (false, null);

        this.Id = (ItemId)id;

        VersionUtc = map["_v"].AsDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        VersionStatus = map["_s"].AsEnum(ItemStatus.Created);
        AbsoluteExpirationUtc = map["absutc"].AsNullableDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        LastUseUtc = map["lutc"].AsDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        SlidingExpirationMinutes = map["semin"].AsInt();
        Data = map["data"].AsString();
        Index = new StringMap();
        (Index as IJsonReadable).ReadAsJson(map["idx"], fromUI, nameBinding);

        return (true, this);
      }

      return (false, null);
    }

    #endregion



  }
}
