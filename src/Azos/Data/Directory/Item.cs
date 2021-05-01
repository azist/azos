/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Azos.Collections;
using Azos.Serialization.JSON;

namespace Azos.Data.Directory
{
  public enum ItemStatus{ Created = 0, Updated = 1, Deleted = 100 }

  /// <summary>
  /// Represents an item stored in a directory
  /// </summary>
  [Serializable]
  public sealed class Item : IJsonWritable, IJsonReadable
  {
    /// <summary>
    /// Provides a dictionary of string:object values, having values one of the types: null|string|int|long|short|sbyte|DateTime
    /// </summary>
    public sealed class AttrMap : AdhocMapDecorator
    {
      public const int MAX_KEY_LEN = 64;
      public const int MAX_VAL_LEN = 0xff;

      internal AttrMap() : base(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)){ }

      //Key can be anything non-blank
      protected override string CheckKey(string key) => key.NonBlankMax(MAX_KEY_LEN);

      protected override object CheckValue(object value)
      {
        Aver.IsTrue(
          value == null ||
          (value is string s && s.NonBlankMax(MAX_VAL_LEN)!=null) ||
          value is int ||
          value is long ||
          value is short ||
          value is sbyte ||
          value is DateTime,
          "Value of type `{0}` is not supported by Item index props which must be one of: `null|string|int|long|short|sbyte|DateTime`".Args(value.GetType().DisplayNameWithExpandedGenericArgs())
        );
        return value;
      }
    }

    /// <summary>
    /// .ctor for ser speedup
    /// </summary>
    internal Item(){ }

    /// <summary>
    /// Creates a new Item instance. You must provide a new unique Id
    /// </summary>
    public Item(EntityId id)
    {
      m_Id = id;
    }

    /// <summary>
    /// Updates the version information of the item before saving a change.
    /// This is called internally by the save framework or while loading data from an external store (such as a database)
    /// </summary>
    internal void SetVersion(GDID gdid, DateTime utcNow, ItemStatus status)
    {
      m_Gdid = gdid;
      m_VersionUtc = m_LastUseUtc = utcNow;
      m_VersionStatus = status;
    }

    #region Fields

    private GDID m_Gdid;
    private EntityId m_Id;
    private DateTime m_VersionUtc;
    private ItemStatus m_VersionStatus;
    private DateTime? m_AbsoluteExpirationUtc;
    private DateTime m_LastUseUtc;
    private string m_Data;

    #endregion

    #region Props

    /// <summary>
    /// Returns system-assigned internal global distributed id of the item used for data sync etc.
    /// </summary>
    public GDID Gdid => m_Gdid;

    /// <summary>
    /// A unique ID of the Item
    /// </summary>
    public EntityId Id => m_Id;

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
    /// Optional future point in time when item expires (disappears). Expressed as a UTC timestamp
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
    /// If greater than zero, sets the sliding expiration life span expressed in seconds. Works together with LastUseUtc
    /// </summary>
    public long SlidingExpirationSec { get; set; }

    /// <summary>
    /// Item's data
    /// </summary>
    public string Data
    {
      get => m_Data;
      set => m_Data = value.NonBlank(nameof(value));
    }

    /// <summary>
    /// Item index entries, an optional map of name=value pairs which will be indexed.
    /// You can use the following types of values: int numerics, DateTime or string. Null signifies no indexing
    /// </summary>
    public AttrMap Index {get; set;}

    /// <summary>
    /// An optional map of ad-hoc name=value pairs which will not be indexed (contrast with Index).
    /// You can use the following types of values: int numerics, DateTime or string. Null signifies no extra data attributes
    /// </summary>
    public AttrMap Props { get; set; }

    #endregion

    #region JSON Serialization
    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options,
          new DictionaryEntry("_id", this.Gdid),
          new DictionaryEntry("_v", this.VersionUtc),
          new DictionaryEntry("_s", this.VersionStatus),
          new DictionaryEntry("id", this.Id),
          new DictionaryEntry("ax", this.AbsoluteExpirationUtc),
          new DictionaryEntry("lu", this.LastUseUtc),
          new DictionaryEntry("sx", this.SlidingExpirationSec),
          new DictionaryEntry("dat", this.Data),
          new DictionaryEntry("idx", Index),
          new DictionaryEntry("pro", Props)
       );
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data == null) return (true, null);
      if (data is JsonDataMap map)
      {
        if (!EntityId.TryParse(map["id"].AsString(), out m_Id)) return (false, null);

        m_Gdid = map["_id"].AsGDID(GDID.ZERO);
        m_VersionUtc = map["_v"].AsDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        m_VersionStatus = map["_s"].AsEnum(ItemStatus.Created);
        AbsoluteExpirationUtc = map["ax"].AsNullableDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        m_LastUseUtc = map["lu"].AsDateTime(styles: System.Globalization.DateTimeStyles.AdjustToUniversal);
        SlidingExpirationSec = map["sx"].AsLong();
        m_Data = map["dat"].AsString();

        var sub = map["idx"];
        if (sub!=null)
        {
          Index = new AttrMap();
          Index.ReadAsJson(sub, fromUI, options);
        }

        sub = map["pro"];
        if (sub != null)
        {
          Props = new AttrMap();
          Props.ReadAsJson(sub, fromUI, options);
        }

        return (true, this);
      }

      return (false, null);
    }

    #endregion
  }
}
