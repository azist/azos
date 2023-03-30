/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Provides reading options for Json datagrams like imposing limits on maximum character length or object nesting depth and counts
  /// </summary>
  public class JsonReadingOptions : IConfigurable
  {
    /// <summary>
    /// MaxDepth property may not exceed this limit
    /// </summary>
    public const int MAX_DEPTH_MAX = 0xff;


    private static readonly JsonReadingOptions s_NoLimits =   new JsonReadingOptions(isSystem: true);
    private static readonly JsonReadingOptions s_DefaultLimits = new JsonReadingOptions(isSystem: true)
    {
      m_CaseSensitiveMaps = true,
      m_MaxDepth = 64,
      m_MaxCharLength = 5 * 1024 * 1024,
      m_MaxObjects = 100_000,
      m_MaxArrays = 100_000,
      m_MaxObjectItems = 1000,
      m_MaxArrayItems = 100_000,
      m_MaxKeyLength = 300,
      m_MaxStringLength = 128 * 1024,
      m_MaxCommentLength = 8 * 1024,
      m_BufferSize = 0,
      m_SegmentTailThresholdPercent = 0f,
      m_SensitiveData = false,
      m_TimeoutMs = 0
    };

    private static readonly JsonReadingOptions s_DefaultLimitsCaseInsensitive = new JsonReadingOptions(isSystem: true, s_DefaultLimits)
    {
      m_CaseSensitiveMaps = false
    };

    /// <summary>
    /// No Limits
    /// </summary>
    public static JsonReadingOptions NoLimits => s_NoLimits;

    /// <summary>
    /// Default permissive Limits
    /// </summary>
    public static JsonReadingOptions DefaultLimits => s_DefaultLimits;

    /// <summary>
    /// Default permissive Limits with case-insensitive json key names
    /// </summary>
    public static JsonReadingOptions DefaultLimitsCaseInsensitive => s_DefaultLimitsCaseInsensitive;

    /// <summary>
    /// Default instance points to `DefaultLimits`
    /// </summary>
    public static JsonReadingOptions Default => s_DefaultLimits;


    public JsonReadingOptions(){ }
    internal JsonReadingOptions(bool isSystem, JsonReadingOptions other = null) : this(other) { m_IsSystem = isSystem; }

    /// <summary>
    /// Creates instance by cloning the existing one, such an immutable system one
    /// </summary>
    public JsonReadingOptions(JsonReadingOptions other)
    {
      if (other == null) return;

      this.m_CaseSensitiveMaps = other.m_CaseSensitiveMaps;
      this.m_MaxDepth       = other.m_MaxDepth;
      this.m_MaxCharLength  = other.m_MaxCharLength;
      this.m_MaxObjects     = other.m_MaxObjects;
      this.m_MaxArrays      = other.m_MaxArrays;
      this.m_MaxObjectItems = other.m_MaxObjectItems;
      this.m_MaxArrayItems  = other.m_MaxArrayItems;
      this.m_MaxKeyLength      = other.m_MaxKeyLength;
      this.m_MaxStringLength   = other.m_MaxStringLength;
      this.m_MaxCommentLength  = other.m_MaxCommentLength;
      this.m_BufferSize     = other.m_BufferSize;
      this.m_SegmentTailThresholdPercent = other.m_SegmentTailThresholdPercent;
      this.m_SensitiveData  = other.m_SensitiveData;
      this.m_TimeoutMs      = other.m_TimeoutMs;
    }


    private bool m_IsSystem;
    private T nonsys<T>(T v) => m_IsSystem ? throw new AzosException(StringConsts.IMMUTABLE_SYS_INSTANCE.Args(nameof(JsonWritingOptions))) : v;

    private bool m_CaseSensitiveMaps = true;
    private int m_MaxDepth = MAX_DEPTH_MAX;
    private int m_MaxCharLength;


    private int m_MaxObjects;//total count
    private int m_MaxArrays;//total count

    private int m_MaxObjectItems;//per every object
    private int m_MaxArrayItems; //per every array
    private int m_MaxKeyLength;     //max length of json key/property name
    private int m_MaxStringLength;  //length per every string
    private int m_MaxCommentLength; //length per every comment string


    private int m_BufferSize;//in bytes - stream source reading
    private float m_SegmentTailThresholdPercent;//
    private bool m_SensitiveData;

    private int m_TimeoutMs;//when set > 0, if exceeded aborts reading. Protect against slowloris


    /// <summary>
    /// True to indicate that this instance is system and is immutable
    /// </summary>
    public bool IsSystem => m_IsSystem;



    /// <summary>
    /// True to use case sensitive property/keys on objects (json maps)
    /// </summary>
    [Config]
    public bool CaseSensitiveMaps
    {
      get => m_CaseSensitiveMaps;
      set => m_CaseSensitiveMaps = nonsys(value);
    }

    /// <summary>
    /// Imposes a maximum limit on json datagram nesting depth
    /// </summary>
    [Config]
    public int MaxDepth
    {
      get => m_MaxDepth;
      set => m_MaxDepth = nonsys(value).KeepBetween(0, MAX_DEPTH_MAX);
    }

    /// <summary>
    /// Imposes a maximum length of the whole json datagram in characters.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxCharLength
    {
      get => m_MaxCharLength;
      set => m_MaxCharLength = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a maximum total number of objects (json maps) in the datagram.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxObjects
    {
      get => m_MaxObjects;
      set => m_MaxObjects = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a maximum total number of arrays in the datagram.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxArrays
    {
      get => m_MaxArrays;
      set => m_MaxArrays = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a limit on the max number of key/value entries in an object (json map).
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxObjectItems
    {
      get => m_MaxObjectItems;
      set => m_MaxObjectItems = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a limit on the max number of items in an array.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxArrayItems
    {
      get => m_MaxArrayItems;
      set => m_MaxArrayItems = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a limit on the max char length of an object key (property name).
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxKeyLength
    {
      get => m_MaxKeyLength;
      set => m_MaxKeyLength = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a limit on the max char length of string value.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxStringLength
    {
      get => m_MaxStringLength;
      set => m_MaxStringLength = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a limit on the max char length of a json comment.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxCommentLength
    {
      get => m_MaxCommentLength;
      set => m_MaxCommentLength = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Provides a size of buffer for content reading, a value of a few kilobytes makes sense.
    /// 0 = use system default
    /// </summary>
    [Config]
    public int BufferSize
    {
      get => m_BufferSize;
      set => m_BufferSize = nonsys(value).KeepBetween(0, CodeAnalysis.Source.StreamSource.MAX_BUFFER_SIZE);
    }

    /// <summary>
    /// Sets the threshold expressed as X% of buffer size before the buffer end.
    /// Affects asynchronous processing.
    /// 0.0f = use system default
    /// </summary>
    [Config]
    public float SegmentTailThresholdPercent
    {
      get => m_SegmentTailThresholdPercent;
      set => m_SegmentTailThresholdPercent = nonsys(value).KeepBetween(0f, CodeAnalysis.Source.StreamSource.MAX_SEG_TAIL_THRESHOLD_PCT);
    }

    /// <summary>
    /// Set to true to give system a hint to clear intermediary resources more aggressively,
    /// e.g. clear intermediary pooled buffers which can get stuck in memory for a long time
    /// </summary>
    [Config]
    public bool SensitiveData
    {
      get => m_SensitiveData;
      set => m_SensitiveData = nonsys(value);
    }

    /// <summary>
    /// When set to above zero value, imposes a total read operation timeout in ms.
    /// This may be used to prevent DDOS attacks like "Slowloris" or detect hardware failures
    /// while reading large datagrams.
    /// 0 = no timeout
    /// </summary>
    [Config]
    public int TimeoutMs
    {
      get => m_TimeoutMs;
      set => m_TimeoutMs = nonsys(value).AtMinimum(0);
    }

    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, nonsys(node));
  }
}
