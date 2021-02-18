/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Apps;
using Azos.Instrumentation;
using Azos.Pile;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Provides IPageCache implementation based on big memory Pile
  /// </summary>
  public sealed class PilePageCache : ApplicationComponent, IPageCache
  {
    private class _entry
    {
      public PageInfo Info;
      public byte[] Data;
    }

    public PilePageCache(ICache cache) : base(cache.CastTo<ICacheImplementation>().App)
    {
      m_Cache = cache;
      Enabled = true;
    }

    public PilePageCache(IApplicationComponent director, ICache cache) : base(director)
    {
      m_Cache = cache.NonNull(nameof(cache));
      Enabled = true;
    }

    private ICache m_Cache;


    /// <summary>
    /// If set to false, temporarily disables cache without losing its contents
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE)]
    public bool Enabled { get; set; }

    /// <summary>
    /// When greater than zero imposes a maximum lifespan for the item
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE)]
    public int LifeTimeSec { get; set; }

    /// <summary>
    /// When set to greater than zero, imposes a maximum number of items stored in a cache per volume
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE)]
    public long MemoryLimit { get; set; }

    public IEnumerable<Guid> VolumeIds
    {
      get
      {
        EnsureObjectNotDisposed();
        return m_Cache.Tables
                .Where(t => t.Name.StartsWith(tblNamePrefix))
                .Select(t => Guid.Parse(t.Name.Replace(tblNamePrefix, string.Empty)));
      }
    }

    public override string ComponentLogTopic => CoreConsts.PILE_TOPIC;

    public void Clear(Guid idVolume)
    {
      EnsureObjectNotDisposed();

      if (idVolume == Guid.Empty)
      {
        m_Cache.PurgeAll();
        return;
      }

      var tbl = getVolumeTable(idVolume, true);
      if (tbl!=null) tbl.Purge();
    }

    public bool Contains(VolumePagePtr pPage)
    {
      EnsureObjectNotDisposed();
      if (!Enabled) return false;
      var tbl = getVolumeTable(pPage.VolumeId);
      return tbl.ContainsKey(pPage.PageId);
    }

    public void Put(VolumePagePtr pPage, PageInfo info, ArraySegment<byte> content)
    {
      EnsureObjectNotDisposed();
      if (!Enabled) return;

      var tbl = getVolumeTable(pPage.VolumeId);
      var item = new _entry
      {
        Info = info,
        Data = new byte[content.Count]
      };

      Array.Copy(content.Array, content.Offset, item.Data, 0, content.Count);

      int? maxAge = LifeTimeSec;
      if (maxAge.Value <= 0) maxAge = null;

      //since pageId can not be less than 1 we use negative pageIds for storing PageInfo without the byte array
      tbl.Put(pPage.PageId, item, maxAge); //store the full item
      tbl.Put(-pPage.PageId, info, maxAge); //store just the pageInfo with reverse index
    }

    public bool TryGet(VolumePagePtr pPage, MemoryStream pageData, out PageInfo info)
    {
      EnsureObjectNotDisposed();
      if (!Enabled)
      {
        info = default(PageInfo);
        return false;
      }

      var tbl = getVolumeTable(pPage.VolumeId);
      var cached = tbl.Get(pPage.PageId) as _entry;
      if (cached == null)
      {
        info = default(PageInfo);
        return false;
      }

      info = cached.Info;
      pageData.Write(cached.Data, 0, cached.Data.Length);
      return true;
    }

    public bool TryGet(VolumePagePtr pPage, out PageInfo info)
    {
      EnsureObjectNotDisposed();
      if (!Enabled)
      {
        info = default(PageInfo);
        return false;
      }

      var tbl = getVolumeTable(pPage.VolumeId);
      var cached = tbl.Get(-pPage.PageId);//the pageInfo is stored with flipped pageId
      if (cached is PageInfo pi)
      {
        info = pi;
        return true;
      }

      info = default(PageInfo);
      return false;
    }




    private string tblNamePrefix => GetType().FullName + "-";
    private Dictionary<Guid, string> m_InternedNames = new Dictionary<Guid, string>();

    private ICacheTable<long> getVolumeTable(Guid id, bool existingOnly = false)
    {
      const int MAX_INTERN_POOL = 1024;
      if (!m_InternedNames.TryGetValue(id, out var tname))//try to convert Guid to string without allocations
      {
        tname = tblNamePrefix + id.ToString();
        var dict =  new Dictionary<Guid, string>(m_InternedNames);
        while (m_InternedNames.Count + 1  > MAX_INTERN_POOL)
        {
          dict.Remove(dict.First().Key);
        }
        dict[id] = tname;
        System.Threading.Thread.MemoryBarrier();
        m_InternedNames = dict;
      }

      var result = existingOnly ? m_Cache.Tables[tname] as ICacheTable<long>
                                : m_Cache.GetOrCreateTable<long>(tname);

      if (result != null)
      {
        // The collision mode needs to be set on default table options, setting it after .ctor has no effect anyway
        //// ....DefaultOptions.CollisionMode = CollisionMode.Durable;
        result.Options.MaximumCapacity = MemoryLimit;
      }
      return result;
    }

  }
}
