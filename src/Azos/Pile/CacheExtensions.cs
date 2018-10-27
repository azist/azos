/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Access;

namespace Azos.Pile
{
  /// <summary>
  /// Provides read/write-through extensions
  /// </summary>
  public static class CacheExtensions
  {
    /// <summary>
    /// Fetches an existing item from cache or null. IsAbsent is true when data was read from cache as an AbsentValue
    /// </summary>
    public static TResult FetchFrom<TKey, TResult>(this ICache cache, TKey key, string tblCache, ICacheParams caching, out bool isAbsent) where TResult : class
    {
      if (caching==null) caching = CacheParams.DefaultCache;

      TResult result = null;

      isAbsent = false;

      if (caching.ReadCacheMaxAgeSec>=0)
      {
        ICacheTable<TKey> tbl = cache.GetOrCreateTable<TKey>(tblCache);
        var cached = tbl.Get(key, caching.ReadCacheMaxAgeSec);
        isAbsent = cached is AbsentValue;
        if (!isAbsent)
          result = cached as TResult;
      }

      return result;
    }

    /// <summary>
    /// Fetches an item through cache
    /// </summary>
    public static TResult FetchThrough<TKey, TResult>(this ICache cache, TKey key, string tblCache, ICacheParams caching, Func<TKey, TResult> fFetch, Func<TKey, TResult, TResult> fFilter = null) where TResult : class
    {
      ICacheTable<TKey> tbl = null;

      if (caching==null) caching = CacheParams.DefaultCache;

      if (caching.ReadCacheMaxAgeSec>=0 || caching.WriteCacheMaxAgeSec>=0)
        tbl = cache.GetOrCreateTable<TKey>(tblCache);

      TResult result = null;

      if (caching.ReadCacheMaxAgeSec>=0)
      {
        var cached = tbl.Get(key, caching.ReadCacheMaxAgeSec);
        if (cached is AbsentValue)
          return null;
        else
          result = cached as TResult;

        if (fFilter != null)
          result = fFilter(key, result);
      }

      if (result!=null) return result;

      result = fFetch(key);

      if (result==null && !caching.CacheAbsentData) return null;

      var wAge = caching.WriteCacheMaxAgeSec;
      if (wAge>=0)
        tbl.Put(key, (object)result ?? AbsentValue.Instance, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);

      return result;
    }

    public static bool DeleteThrough<TKey>(this ICache cache, TKey key, string tblCache, ICacheParams caching, Func<TKey, bool> fDelete)
    {
      if (caching==null) caching = CacheParams.DefaultCache;

      var wAge = caching.WriteCacheMaxAgeSec;
      var tbl = cache.GetOrCreateTable<TKey>(tblCache);

      if (caching.CacheAbsentData && wAge>=0)
        tbl.Put(key, AbsentValue.Instance, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);
      else
        tbl.Remove(key);

      return fDelete(key);
    }

    public static int SaveThrough<TKey, TData>(this ICache cache, TKey key, TData data, string tblCache, ICacheParams caching, Func<TKey, TData, int> fSave) where TData : class
    {
      if (data==null) return 0;

      if (caching==null) caching = CacheParams.DefaultCache;

      var result = fSave(key, data);

      var wAge = caching.WriteCacheMaxAgeSec;
      if (wAge>=0)
      {
        var tbl = cache.GetOrCreateTable<TKey>(tblCache);
        tbl.Put(key, data, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);
      }

      return result;
    }

  }
}
