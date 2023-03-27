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
  /// Provides reading options for Json datagrams like maximum character length or object nesting depth
  /// </summary>
  public class JsonReadingOptions : IConfigurable
  {
    /// <summary>
    /// MaxDepth property may not exceed this limit
    /// </summary>
    public const int MAX_DEPTH_MAX = 0xff;


    private static JsonReadingOptions s_Default =   new JsonReadingOptions(isSystem: true);

    /// <summary>
    /// Default instance
    /// </summary>
    public static JsonReadingOptions Default => s_Default;

    public JsonReadingOptions(){ }
    internal JsonReadingOptions(bool isSystem) { m_IsSystem = isSystem; }

    public JsonReadingOptions(JsonReadingOptions other)
    {
      if (other==null) return;

      this.MaxDepth = other.MaxDepth;

    }


    private bool m_IsSystem;
    private T nonsys<T>(T v) => m_IsSystem ? throw new AzosException(StringConsts.IMMUTABLE_SYS_INSTANCE.Args(nameof(JsonWritingOptions))) : v;

    private int m_MaxDepth;
    private int m_MaxCharLength;


    private int m_MaxObjects;//total count
    private int m_MaxArrays;//total count

    private int m_MaxObjectItems;//per every object
    private int m_MaxArrayItems; //per every array
    private int m_MaxKeyLen;     //max length of json key/property name
    private int m_MaxStringLen;  //length per every string
    private int m_MaxCommentLen; //length per every comment string


    private int m_BufferSize;//in bytes - stream source reading
    private float m_SegmentTailThresholdPercent;//
    private bool m_SensitiveData;

    private int m_TimeoutMs;//when set > 0, if exceeded aborts reading. Protect against slowloris


    /// <summary>
    /// True to indicate that this instance is system and is immutable
    /// </summary>
    public bool IsSystem => m_IsSystem;


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
    public int MaxKeyLen
    {
      get => m_MaxKeyLen;
      set => m_MaxKeyLen = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a limit on the max char length of string value.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxStringLen
    {
      get => m_MaxStringLen;
      set => m_MaxStringLen = nonsys(value).AtMinimum(0);
    }

    /// <summary>
    /// Imposes a limit on the max char length of a json comment.
    /// 0 = no limit
    /// </summary>
    [Config]
    public int MaxCommentLen
    {
      get => m_MaxCommentLen;
      set => m_MaxCommentLen = nonsys(value).AtMinimum(0);
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
      set => m_TimeoutMs = nonsys(value);
    }

    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, nonsys(node));
  }
}
