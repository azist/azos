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
    /// <summary>
    /// .ctor for ser speedup
    /// </summary>
    internal Item(){ }

    /// <summary>
    /// Creates a new Item instance. You must provide a new unique Id
    /// </summary>
    public Item(ItemId id)
    {
      m_Id = id;
    }

    /// <summary>
    /// Updates the version information of the item before saving a change.
    /// This is called internally by the save framework
    /// </summary>
    internal void SetVersion(DateTime utcNow, ItemStatus status)
    {
      m_VersionUtc = m_LastUseUtc = utcNow;
      m_VersionStatus = status;
    }

    #region Fields
      private ItemId m_Id;
      private DateTime m_VersionUtc;
      private ItemStatus m_VersionStatus;
      private DateTime? m_AbsoluteExpirationUtc;
      private DateTime m_LastUseUtc;
      private string m_Data;
      private StringMap m_Index = new StringMap(false);
    #endregion


    #region Props

    /// <summary>
    /// A unique ID of the Item
    /// </summary>
    public ItemId Id => m_Id;

    /// <summary>
    /// Returns true to indicate that this instance has been assigned aversion,
    /// that is- it is not a new instance just allocated as it was saved before
    /// </summary>
    public bool IsVersioned => m_VersionUtc != default(DateTime);

    /// <summary>The UTC timestamp of this version </summary>
    public DateTime VersionUtc => m_VersionUtc;

    /// <summary>The status of this item: Created/Updated/Deleted </summary>
    public ItemStatus VersionStatus => m_VersionStatus;


    /// <summary>
    /// Optional future point in time when item expires (disappears). Expressed as UTC timestamp
    /// </summary>
    public DateTime? AbsoluteExpirationUtc
    {
      get => m_AbsoluteExpirationUtc;
      set
      {
        if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
          throw new DataException(StringConsts.ARGUMENT_ERROR + "Item.AbsoluteExpirationUtc.set(Kind!=Utc)");

        m_AbsoluteExpirationUtc = value;
      }
    }


    /// <summary>
    /// The UTC timestamp of the last use, such as Create/Update/Get(touch=true) or Touch(id). This is used for optional SlidingExpirationMinutes
    /// </summary>
    public DateTime LastUseUtc => m_LastUseUtc;

    /// <summary>
    /// If greater than zero, sets the sliding expiration life span. Works together with LastUseUtc
    /// </summary>
    public int SlidingExpirationMinutes { get; set; }

    /// <summary>
    /// Item's data
    /// </summary>
    public string Data
    {
      get => m_Data;
      set => m_Data = value.NonBlank(nameof(value));
    }

    /// <summary>
    /// Item index entries, a list of name=value pairs which will be indexed
    /// </summary>
    public StringMap Index => m_Index;

    #endregion

    #region JSON Serialization
    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options,
          new DictionaryEntry("id", this.Id),
          new DictionaryEntry("_v", this.VersionUtc),
          new DictionaryEntry("_s", this.VersionStatus),
          new DictionaryEntry("ax", this.AbsoluteExpirationUtc),
          new DictionaryEntry("lu", this.LastUseUtc),
          new DictionaryEntry("sx", this.SlidingExpirationMinutes),
          new DictionaryEntry("dat", this.Data),
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

        this.m_Id = (ItemId)id;

        m_VersionUtc = map["_v"].AsDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        m_VersionStatus = map["_s"].AsEnum(ItemStatus.Created);
        AbsoluteExpirationUtc = map["ax"].AsNullableDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        m_LastUseUtc = map["lu"].AsDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        SlidingExpirationMinutes = map["sx"].AsInt();
        Data = map["dat"].AsString();
        (Index as IJsonReadable).ReadAsJson(map["idx"], fromUI, nameBinding);

        return (true, this);
      }

      return (false, null);
    }

    #endregion
  }
}
