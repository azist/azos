/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Log;

namespace Azos.Data.Access.Cache
{
  /// <summary>
  /// Stores cached item (such as a business object) along with additional caching information about it.
  /// The instance of this class gets reused for the same Key, so Key is immutable field.
  /// The instance of this class is returned by table.Get(key...) so the calling thread may see different Value property with time
  /// as it may be dynamically changed by some other thread (the newer value for the same Key)
  /// </summary>
  public sealed class CacheRec : Bucketed
  {
    internal CacheRec(ulong key, object data, int maxAgeSec, int priority, DateTime? absoluteExpirationUTC)
    {
      Key = key;
      m_Value = data;
      m_MaxAgeSec = maxAgeSec > 0 ? maxAgeSec : 0;
      m_Priority = priority;
      m_AbsoluteExpirationUTC = absoluteExpirationUTC;
    }

    /// <summary>
    /// Used as a "constructor" that reuses this class instance to lessen GC burden
    /// </summary>
    internal void ReuseCTOR(object data, int maxAgeSec, int priority, DateTime? absoluteExpirationUTC)
    {
      _reset();
      System.Threading.Thread.MemoryBarrier();
      m_Value = data;
      m_MaxAgeSec = maxAgeSec > 0 ? maxAgeSec : 0;
      m_Priority = priority;
      m_AbsoluteExpirationUTC = absoluteExpirationUTC;
    }

    protected override void Destructor()
    {
      const string FROM = "CacheRec.Destructor()";

      if (m_Value != null)
        if (m_Value is IDisposable)
          try
          {
            ((IDisposable)m_Value).Dispose();
          }
          catch (Exception error)
          {
            Trace.Write(StringConsts.CACHE_RECORD_ITEM_DISPOSE_ERROR.Args(FROM, error.ToMessageWithType()), FROM, MessageType.Error, topic: CoreConsts.CACHE_TOPIC);
          }
    }

    internal DateTime m_CreateDate;//set by sweep thread on first access
    internal volatile int m_AgeSec; //needed not to subtract dates on every Get() from cache

    internal DateTime? m_AbsoluteExpirationUTC;
    internal volatile int m_HitCount;

    internal volatile int m_MaxAgeSec;
    internal volatile int m_Priority;

    /// <summary>
    /// Key is immutable because CacheRec is returned by table.Get(key)
    /// </summary>
    public readonly ulong Key;

    internal volatile object m_Value;

    /// <summary>
    /// Stores arbitrary information about this item
    /// </summary>
    public volatile object Metadata;

    private void _reset()
    {
      m_Value = null;
      Metadata = null;

      m_CreateDate = default(DateTime);
      m_AgeSec = 0;
      m_AbsoluteExpirationUTC = null;
      m_HitCount = 0;
      m_MaxAgeSec = 0;
      m_Priority = 0;
    }

    /// <summary>
    /// Returns an approximate age of the item in seconds - an interval since this item was put into the store for the last time
    /// </summary>
    public int AgeSec => m_AgeSec;

    /// <summary>
    /// Returns positive integer that specifies the maximum lifespan duration for this item expressed in seconds, or zero.
    /// Zero is returned when this item does not have specific lifespan defined and will be evicted from cache
    ///  per lifespan specified on the table level: Table.MaxAgeSec (default)
    /// </summary>
    public int MaxAgeSec => m_MaxAgeSec;

    /// <summary>
    /// Returns absolute expiration timestamp for this item or null
    /// </summary>
    public DateTime? AbsoluteExpirationUTC => m_AbsoluteExpirationUTC;

    /// <summary>
    /// Returns an integer value that dictates item priority relative to other items in the same table.
    /// Priorities play a role during cache collision as an item with higher priority is not going to be replaced
    ///  by an item with lower priority
    /// </summary>
    public int Priority => m_Priority;

    /// <summary>
    /// Returns how many times this cache item was hit (resulted in successful read)
    /// </summary>
    public int HitCount => m_HitCount;


    /// <summary>
    /// Returns the value - a reference to cached item such as a business object
    /// </summary>
    public object Value => m_Value;


    /// <summary>
    /// Returns typecast value
    /// </summary>
    public T ValueAs<T>() => (T)Value;

  }
}
