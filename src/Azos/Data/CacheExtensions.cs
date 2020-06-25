/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Pile;
using Azos.Data.Access;

namespace Azos.Data
{
  /// <summary>
  /// Provides cache read/write-through extensions
  /// </summary>
  public static class CacheExtensions
  {
    /// <summary>
    /// This is an async version of FetchFrom. This cache operation is only nominally asynchronous as it is performed in-memory and hence CPU-bound.
    /// Fetches an existing item from cache or null. IsAbsent is true when data was read from cache as an AbsentValue.
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TResult">Type of the returned result value</typeparam>
    /// <param name="cache">Cache instance to operate on</param>
    /// <param name="key">Key to read</param>
    /// <param name="tblCache">Name of a cache table within a cache</param>
    /// <param name="caching">Caching options, or null for defaults</param>
    /// <param name="isAbsent">Returns true when value was found, but it is an AbsentValue instance</param>
    /// <returns>Cached reference type value, or null if not found or absent</returns>
    public static Task<TResult> FetchFromAsync<TKey, TResult>(this ICache cache,
                                                              TKey key,
                                                              string tblCache,
                                                              ICacheParams caching,
                                                              out bool isAbsent) where TResult : class
    => Task.FromResult(cache.FetchFrom<TKey, TResult>(key, tblCache, caching, out isAbsent));

    /// <summary>
    /// Fetches an existing item from cache or null. IsAbsent is true when data was read from cache as an AbsentValue
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TResult">Type of the returned result value</typeparam>
    /// <param name="cache">Cache instance to operate on</param>
    /// <param name="key">Key to read</param>
    /// <param name="tblCache">Name of a cache table within a cache</param>
    /// <param name="caching">Caching options, or null for defaults</param>
    /// <param name="isAbsent">Returns true when value was found, but it is an AbsentValue instance</param>
    /// <returns>Cached reference type value, or null if not found or absent</returns>
    public static TResult FetchFrom<TKey, TResult>(this ICache cache,
                                                   TKey key,
                                                   string tblCache,
                                                   ICacheParams caching,
                                                   out bool isAbsent) where TResult : class
    {
      cache.NonNull(nameof(cache));

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
    /// Fetches an item through cache - if the item exists and satisfies the `ICacheParams` (and optional `fFilter` functor) then it is immediately returned
    /// to the caller. Otherwise, calls the `fFetch` functor to perform the actual fetch of a value by key, and then puts the result in cache
    /// according to `ICacheParams`
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TResult">Type of the result value</typeparam>
    /// <param name="cache">Non-null ICache instance to operate on</param>
    /// <param name="key">Key value</param>
    /// <param name="tblCache">Name of cache table</param>
    /// <param name="caching">Caching options, or null for defaults</param>
    /// <param name="fFetch">Required functor that performs actual fetch when the value is NOT found in cache</param>
    /// <param name="fFilter">Optional functor - additional filter applied to existing values</param>
    /// <param name="extraPut">Optional functor (cache, TResult, PilePointer, ICacheParams) which can be used to add the piled value to other cache tables (indexes) </param>
    /// <returns>Cached reference type value, or null if not found or absent</returns>
    public static TResult FetchThrough<TKey, TResult>(this ICache cache,
                                                      TKey key,
                                                      string tblCache,
                                                      ICacheParams caching,
                                                      Func<TKey, TResult> fFetch,
                                                      Func<TKey, TResult, TResult> fFilter = null,
                                                      Action<ICache, TResult, PilePointer, ICacheParams> extraPut = null
                                                      ) where TResult : class
    {
      cache.NonNull(nameof(cache));

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

      result = fFetch.NonNull(nameof(fFetch))(key);

      if (result==null && !caching.CacheAbsentData) return null;

      var wAge = caching.WriteCacheMaxAgeSec;
      if (wAge >=0 )
      {
        tbl.Put(key, (object)result ?? AbsentValue.Instance, out var ptr, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);
        extraPut?.Invoke(cache, result, ptr, caching);
      }

      return result;
    }


    /// <summary>
    /// Asynchronously fetches an item through cache - if the item exists and satisfies the `ICacheParams` (and optional `fFilter` functor) then it is
    /// immediately (synchronously) returned to the caller. Otherwise, calls the `fFetch` async functor to perform the actual fetch of a value by key,
    /// and then puts the result in cache according to `ICacheParams`
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TResult">Type of the result value</typeparam>
    /// <param name="cache">Non-null ICache instance to operate on</param>
    /// <param name="key">Key value</param>
    /// <param name="tblCache">Name of cache table</param>
    /// <param name="caching">Caching options, or null for defaults</param>
    /// <param name="fFetch">Required async functor that performs actual fetch when the value is NOT found in cache</param>
    /// <param name="fFilter">Optional functor - additional filter applied to existing values</param>
    /// <param name="extraPut">Optional functor (cache, TResult, PilePointer, ICacheParams) which can be used to add the piled value to other cache tables (indexes) </param>
    /// <returns>Cached reference type value, or null if not found or absent</returns>
    public static async Task<TResult> FetchThroughAsync<TKey, TResult>(this ICache cache,
                                                           TKey key,
                                                           string tblCache,
                                                           ICacheParams caching,
                                                           Func<TKey, Task<TResult>> fFetch,
                                                           Func<TKey, TResult, TResult> fFilter = null,
                                                           Action<ICache, TResult, PilePointer, ICacheParams> extraPut = null
                                                           ) where TResult : class
    {
      cache.NonNull(nameof(cache));

      ICacheTable<TKey> tbl = null;

      if (caching == null) caching = CacheParams.DefaultCache;

      if (caching.ReadCacheMaxAgeSec >= 0 || caching.WriteCacheMaxAgeSec >= 0)
        tbl = cache.GetOrCreateTable<TKey>(tblCache);

      TResult result = null;

      if (caching.ReadCacheMaxAgeSec >= 0)
      {
        var cached = tbl.Get(key, caching.ReadCacheMaxAgeSec);
        if (cached is AbsentValue)
          return null;
        else
          result = cached as TResult;

        if (fFilter != null)
          result = fFilter(key, result);
      }

      if (result != null) return result;

      result = await fFetch.NonNull(nameof(fFetch))(key);//<-- only fFetch is IO-bound hence asynchronous

      if (result == null && !caching.CacheAbsentData) return null;

      var wAge = caching.WriteCacheMaxAgeSec;
      if (wAge >= 0)
      {
        tbl.Put(key, (object)result ?? AbsentValue.Instance, out var ptr, wAge > 0 ? wAge : (int?)null, caching.WriteCachePriority);
        extraPut?.Invoke(cache, result, ptr, caching);
      }

      return result;
    }


    /// <summary>
    /// Deletes an item from cache and underlying store by calling the supplied functor
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <param name="cache">Non-null cache instance to delete from</param>
    /// <param name="key">Key value</param>
    /// <param name="tblCache">The name of a cache table</param>
    /// <param name="caching">Caching options, or null for defaults</param>
    /// <param name="fDelete">Synchronous functor that performs backend deletion</param>
    /// <returns>True when value was deleted from the backing store</returns>
    public static bool DeleteThrough<TKey>(this ICache cache, TKey key, string tblCache, ICacheParams caching, Func<TKey, bool> fDelete)
    {
      cache.NonNull(nameof(cache));

      if (caching==null) caching = CacheParams.DefaultCache;

      var wAge = caching.WriteCacheMaxAgeSec;
      var tbl = cache.GetOrCreateTable<TKey>(tblCache);

      if (caching.CacheAbsentData && wAge>=0)
        tbl.Put(key, AbsentValue.Instance, wAge >0 ? wAge : (int?)null, caching.WriteCachePriority);
      else
        tbl.Remove(key);

      return fDelete.NonNull(nameof(fDelete))(key);
    }

    /// <summary>
    /// Deletes an item from cache and underlying store by calling the supplied asynchronous functor
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <param name="cache">Non-null cache instance to delete from</param>
    /// <param name="key">Key value</param>
    /// <param name="tblCache">The name of a cache table</param>
    /// <param name="caching">Caching options, or null for defaults</param>
    /// <param name="fDelete">Asynchronous functor that performs backend deletion</param>
    /// <returns>True when value was deleted from the backing store</returns>
    public static async Task<bool> DeleteThroughAsync<TKey>(this ICache cache, TKey key, string tblCache, ICacheParams caching, Func<TKey, Task<bool>> fDelete)
    {
      cache.NonNull(nameof(cache));

      if (caching == null) caching = CacheParams.DefaultCache;

      var wAge = caching.WriteCacheMaxAgeSec;
      var tbl = cache.GetOrCreateTable<TKey>(tblCache);

      if (caching.CacheAbsentData && wAge >= 0)
        tbl.Put(key, AbsentValue.Instance, wAge > 0 ? wAge : (int?)null, caching.WriteCachePriority);
      else
        tbl.Remove(key);

      return await fDelete.NonNull(nameof(fDelete))(key);
    }

    /// <summary>
    /// Saves an item through cache: call the `fSave` functor synchronously then saves the result into cache table
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TData">Type of reference type value to save</typeparam>
    /// <typeparam name="TSaveResult">Type of save result</typeparam>
    /// <param name="cache">Cache to operate on</param>
    /// <param name="key">Key value</param>
    /// <param name="data">Reference type value to save. May not be null or AbsentValue</param>
    /// <param name="tblCache">The name of a cache table</param>
    /// <param name="caching">Caching options, or null for default</param>
    /// <param name="fSave">Synchronous functor that performs backend save</param>
    /// <param name="extraPut">Optional functor (cache, TData, PilePointer, ICacheParams) which can be used to add the piled value to other cache tables (indexes) </param>
    /// <returns>A result of the call to `fSave` functor</returns>
    public static TSaveResult SaveThrough<TKey, TData, TSaveResult>(this ICache cache,
                                                                     TKey key,
                                                                     TData data,
                                                                     string tblCache,
                                                                     ICacheParams caching,
                                                                     Func<TKey, TData, TSaveResult> fSave,
                                                                     Action<ICache, TData, PilePointer, ICacheParams> extraPut = null
                                                                     ) where TData : class
    {
      cache.NonNull(nameof(cache));

      if (data== null || data is AbsentValue) throw new DataAccessException(StringConsts.ARGUMENT_ERROR + "{0}(data ==null || is AbsentValue)".Args(nameof(SaveThrough)));

      if (caching==null) caching = CacheParams.DefaultCache;

      var result = fSave.NonNull(nameof(fSave))(key, data);

      var wAge = caching.WriteCacheMaxAgeSec;
      if (wAge >= 0)
      {
        var tbl = cache.GetOrCreateTable<TKey>(tblCache);
        tbl.Put(key, data, out var ptr, wAge > 0 ? wAge : (int?)null, caching.WriteCachePriority);
        extraPut?.Invoke(cache, data, ptr, caching);
      }

      return result;
    }

    /// <summary>
    /// Saves an item through cache: call the `fSave` functor asynchronously then synchronously saves the result into cache table
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TData">Type of reference type value to save</typeparam>
    /// <typeparam name="TSaveResult">Type of save result</typeparam>
    /// <param name="cache">Cache to operate on</param>
    /// <param name="key">Key value</param>
    /// <param name="data">Reference type value to save. May not be null or AbsentValue</param>
    /// <param name="tblCache">The name of a cache table</param>
    /// <param name="caching">Caching options, or null for default</param>
    /// <param name="fSave">Synchronous functor that performs backend save</param>
    /// <param name="extraPut">Optional functor (cache, TData, PilePointer, ICacheParams) which can be used to add the piled value to other cache tables (indexes) </param>
    /// <returns>A result of the call to `fSave` functor</returns>
    public static async Task<TSaveResult> SaveThroughAsync<TKey, TData, TSaveResult>(this ICache cache,
                                                                     TKey key,
                                                                     TData data,
                                                                     string tblCache,
                                                                     ICacheParams caching,
                                                                     Func<TKey, TData, Task<TSaveResult>> fSave,
                                                                     Action<ICache, TData, PilePointer, ICacheParams> extraPut = null
                                                                     ) where TData : class
    {
      cache.NonNull(nameof(cache));

      if (data == null || data is AbsentValue) throw new DataAccessException(StringConsts.ARGUMENT_ERROR + "{0}(data ==null || is AbsentValue)".Args(nameof(SaveThrough)));

      if (caching == null) caching = CacheParams.DefaultCache;

      var result = await fSave.NonNull(nameof(fSave))(key, data);

      var wAge = caching.WriteCacheMaxAgeSec;
      if (wAge >= 0)
      {
        var tbl = cache.GetOrCreateTable<TKey>(tblCache);
        tbl.Put(key, data, out var ptr, wAge > 0 ? wAge : (int?)null, caching.WriteCachePriority);
        extraPut?.Invoke(cache, data, ptr, caching);
      }

      return result;
    }

  }
}
