/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Access;
using Azos.Instrumentation;

namespace Azos.Sky.Cms.Default
{
  /// <summary>
  /// Provides default implementation of ICmsFacade by providing caching and delegating
  /// the actual work into the CmsSource abstraction. The implementation is thread safe
  /// </summary>
  public sealed class CmsFacade : ModuleBase, ICmsFacade
  {
    #region CONSTS
    public const string CONFIG_SOURCE_SECTION = "source";


    // Specifies how often the visit event happens
    private const int VISIT_EVENT_INTERVAL_MSEC = 5231;

    /// <summary>
    /// Provides default value for caching.
    /// Specifies in seconds for how long the content remains in cache
    /// </summary>
    public const int DEFAULT_CACHE_SEC = 60;

    /// <summary>
    /// Defines how much smoothing the fetch latency filter does - the lower the number the more smoothing is done.
    /// Smoothing makes the gauge insensitive to some seldom delays that may happen every now and then
    /// while ICmsSource performs actual fetching from the external system
    /// </summary>
    private const float FETCH_LATENCY_EMA_FILTER = 0.0007f;
    private const float FETCH_LATENCY_EMA_FILTER_INVERSE = 1f - FETCH_LATENCY_EMA_FILTER;

    #endregion

    #region .ctor
    /// <summary> Infrastructure .ctor used by the app </summary>
    public CmsFacade(IApplication application) : base(application) => ctor();
    /// <summary> Infrastructure .ctor used by the app </summary>
    public CmsFacade(IModule parent) : base(parent) => ctor();

    private void ctor()
    {
      for (var i = 0; i < m_Data.Length; i++) m_Data[i] = new dict();
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Source);
      base.Destructor();
    }
    #endregion

    #region Fields
    private Azos.Time.Event m_VisitEvent;
    private Dictionary<idiso, Task<Content>> m_PendingFetches = new Dictionary<idiso, Task<Content>>();
    private Dictionary<string, IEnumerable<LangInfo>> m_LangData;
    private ICmsSource m_Source;


    private float m_stat_ContentFetchLatencyMs;

    private NamedInterlocked m_stat_RequestCount = new NamedInterlocked();
    private NamedInterlocked m_stat_ResponseCount = new NamedInterlocked();

    private long m_stat_CacheRecordCount;
    private long m_stat_CacheExpiredRecordCount;
    private long m_stat_CacheHitCount;
    private long m_stat_CacheMissCount;
    #endregion

    #region Properties

    /// <summary> False as this module is a dynamically injected one </summary>
    public override bool IsHardcodedModule => false;

    /// <summary> Provides topic for logging made in this component </summary>
    public override string ComponentLogTopic => CoreConsts.CMS_TOPIC;


    public LangInfo DefaultGlobalLanguage => LangInfo.GENERIC_ENGLISH;

    /// <summary>
    /// Specifies the cache presence timespan expressed in seconds.
    /// The negative value disables the cache (not recommended), zero = DEFAULT_CACHE_SEC
    /// </summary>
    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_CACHE)]
    public int CacheSpanSec { get; set; }
    #endregion

    #region Public ICmsFacade Implementation


    /// <summary>
    /// Implements <see cref="ICmsFacade.GetAllPortalsAsync"/>
    /// </summary>
    public async Task<IEnumerable<string>> GetAllPortalsAsync()
    {
      await needLangData().ConfigureAwait(false);

      if (InstrumentationEnabled)
      {
        m_stat_RequestCount.IncrementLong(Datum.UNSPECIFIED_SOURCE);
        m_stat_ResponseCount.IncrementLong(Datum.UNSPECIFIED_SOURCE);
      }

      return m_LangData.Keys;
    }

    /// <summary>
    /// Implements <see cref="ICmsFacade.GetAllSupportedLanguagesAsync(string)"/>
    /// </summary>
    public async Task<IEnumerable<LangInfo>> GetAllSupportedLanguagesAsync(string portal)
    {
      portal.NonBlank(nameof(portal));

      await needLangData().ConfigureAwait(false);

      if (InstrumentationEnabled)
        m_stat_RequestCount.IncrementLong(portal);

      if (m_LangData.TryGetValue(portal, out var langs))
      {
        if (InstrumentationEnabled)
          m_stat_ResponseCount.IncrementLong(portal);
        return langs;
      }

      throw new CmsException($"{nameof(GetAllSupportedLanguagesAsync)}(): Portal `{portal}` was not found");
    }

    /// <summary>
    /// Implements <see cref="ICmsFacade.GetContentAsync(ContentId, Atom?, ICacheParams)"/>
    /// </summary>
    public async Task<Content> GetContentAsync(ContentId id, Atom? isoLang = null, ICacheParams caching = null)
    {
      if (!id.IsAssigned)
        throw new CmsException($"{StringConsts.ARGUMENT_ERROR} {nameof(CmsFacade)}.{nameof(GetContentAsync)}(!id.IsAssigned)");

      if (!isoLang.HasValue || isoLang.Value.IsZero) isoLang = this.DefaultGlobalLanguage.ISO;

      if (caching == null) caching = getEffectiveCaching();

      if (InstrumentationEnabled)
      {
        m_stat_RequestCount.IncrementLong(Datum.UNSPECIFIED_SOURCE);
        m_stat_RequestCount.IncrementLong($"{id.Portal}:${isoLang}");
      }

      var now = App.TimeSource.UTCNow;
      var existing = tryLookupExisting(id, isoLang.Value, now, caching);
      if (existing != null)
      {
        if (InstrumentationEnabled)
        {
          Interlocked.Increment(ref m_stat_CacheHitCount);
          m_stat_ResponseCount.IncrementLong(Datum.UNSPECIFIED_SOURCE);
          m_stat_ResponseCount.IncrementLong($"{id.Portal}:${isoLang}");
        }
        return existing;
      }

      if (InstrumentationEnabled)
      {
        Interlocked.Increment(ref m_stat_CacheMissCount);
      }

      var fetched = await fetchAndCacheContent(id, isoLang.Value, now, caching).ConfigureAwait(false);

      if (InstrumentationEnabled && fetched!=null)
      {
        m_stat_ResponseCount.IncrementLong(Datum.UNSPECIFIED_SOURCE);
        m_stat_ResponseCount.IncrementLong($"{id.Portal}:${isoLang}");
      }

      return fetched;
    }

    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Source);
      m_Source = FactoryUtils.MakeAndConfigureDirectedComponent<ICmsSource>(this, node[CONFIG_SOURCE_SECTION], typeof(FileSystemCmsSource));
    }

    protected override bool DoApplicationAfterInit()
    {
      m_VisitEvent = new Time.Event(App.EventTimer,
                                       name: nameof(CmsFacade),
                                       body: e => visit(),
                                       interval: TimeSpan.FromMilliseconds(VISIT_EVENT_INTERVAL_MSEC));
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      DisposeAndNull(ref m_VisitEvent);
      return base.DoApplicationBeforeCleanup();
    }

    #endregion

    #region .pvt implementation


    private Task m_PendingLangDataFetch;

    private async Task needLangData()
    {
      if (m_LangData != null) return;//already fetched

      async Task fetch()
      {
        await Task.Yield();
        m_LangData = await m_Source.FetchAllLangDataAsync().ConfigureAwait(false);
      }

      if (m_PendingLangDataFetch==null)
         m_PendingLangDataFetch = fetch();

      await m_PendingLangDataFetch.ConfigureAwait(false);
      return;
    }



    //Buckets are used for thread lock granularity
    private const int BUCKET_COUNT = 37;//must be prime

    //private vector{ContentId, ISO}
    private struct idiso : IEquatable<idiso>
    {
      public idiso(ContentId id, Atom isoLang)
      {
        Id = id;
        IsoLang = isoLang;
      }
      public ContentId Id;
      public Atom IsoLang;

      public bool Equals(idiso other)   => this.Id.Equals(other.Id) && this.IsoLang==other.IsoLang;
      public override int GetHashCode() => Id.GetHashCode() ^ IsoLang.GetHashCode();
    }

    private class dict : Dictionary<idiso, (DateTime sd, object content)> { }

    //the array is used not to interlock on a single object
    private dict[] m_Data = new dict[BUCKET_COUNT];


    private ICacheParams getEffectiveCaching()
     => CacheSpanSec < 0 ? CacheParams.NoCache
                         : CacheSpanSec == 0
                         ? CacheParams.ReadWriteSec(DEFAULT_CACHE_SEC)
                         : CacheParams.ReadWriteSec(CacheSpanSec);

    private dict getDict(ContentId id)
     => m_Data[(id.GetHashCode() & CoreConsts.ABS_HASH_MASK) % BUCKET_COUNT];

    //this method is 100% in-memory CPU-bound, no need for async
    private Content tryLookupExisting(ContentId id, Atom isoLang, DateTime now, ICacheParams caching)
    {
      if (caching.ReadCacheMaxAgeSec < 0) return null;//Bypass cache altogether

      var key = new idiso(id, isoLang);

      var data = getDict(id);
      (DateTime sd, object content) tuple;

      lock (data)
      {
        if (!data.TryGetValue(key, out tuple)) return null;
      }

      if (tuple.content is AbsentValue) return null;

      if (tuple.content is Content existing)
      {
        if (caching.ReadCacheMaxAgeSec >= (now - tuple.sd).TotalSeconds) return existing;
        return null;//expired
      }
      else //safeguard
        throw new AvermentException($"Key '{id}' yields cache dict entry '{tuple.content?.GetType().Name}' which is neither a Content|AbsentValue");
    }

    private async Task<Content> fetchAndCacheContent(ContentId id, Atom isoLang, DateTime now, ICacheParams caching)
    {
      var key = new idiso(id, isoLang);
      var added = false;
      try
      {
        Task<Content> task;
        //multiple callers may request the same content which is not in cache
        //we DO NOT WANT to make IO calls > 1 for the same content, but rather wait
        //on an already pending request
        lock (m_PendingFetches)
        {
          if (!m_PendingFetches.TryGetValue(key, out task))
          {
            task = fetchAndCacheContentBody(id, isoLang, now, caching);//under lock, must be very fast
            m_PendingFetches.Add(key, task);
            added = true;
          }
        }
        return await task.ConfigureAwait(false);//await either existing or just created I/O task
      }
      finally
      {
        if (added) lock (m_PendingFetches) m_PendingFetches.Remove(key);
      }
    }

    //this call must return instantly, the task may take long time to complete
    private async Task<Content> fetchAndCacheContentBody(ContentId id, Atom isoLang, DateTime now, ICacheParams caching)
    {
      await Task.Yield();
      var key = new idiso(id, isoLang);
      Content content;

      var chronometer = System.Diagnostics.Stopwatch.StartNew();

      content = await m_Source.FetchContentAsync(id, isoLang, now, caching).ConfigureAwait(false);//this is an I/O bound, possibly long operation

      //apply EMA smoothing
      m_stat_ContentFetchLatencyMs = (FETCH_LATENCY_EMA_FILTER * chronometer.ElapsedMilliseconds) +
                                     (FETCH_LATENCY_EMA_FILTER_INVERSE * m_stat_ContentFetchLatencyMs);

      if (caching.WriteCacheMaxAgeSec > 0 && (content!=null || caching.CacheAbsentData))
      {
        var data = getDict(id);

        lock (data)
          data[key] = (sd: now, content: content == null ? (object)AbsentValue.Instance : content);
      }
      return content;
    }

    //This is called periodically by the event timer
    private void visit()
    {
      try
      {
       expireCache();
      }
      catch(Exception error)
      {
        WriteLog(Azos.Log.MessageType.CatastrophicError, nameof(visit), $"{nameof(expireCache)}() leaked: {error.ToMessageWithType()}", error);
      }

      try
      {
        dumpStats();
      }
      catch (Exception error)
      {
        WriteLog(Azos.Log.MessageType.CatastrophicError, nameof(visit), $"{nameof(dumpStats)}() leaked: {error.ToMessageWithType()}", error);
      }
    }


    private void expireCache()
    {
      var span = CacheSpanSec;
      if (span == 0) span = DEFAULT_CACHE_SEC;

      var countTotal = 0;
      var countDeleted = 0;
      var now = App.TimeSource.UTCNow;
      foreach(var dict in m_Data)
      {
        lock(dict)//this lock is very fast, we do not do any IO operations under it
        {
          countTotal += dict.Count;

          if (span<0)
          {
            countDeleted += dict.Count;
            dict.Clear();
            continue;
          }

          var toDelete = new List<idiso>();
          foreach(var kvp in dict)
          {
            var sd = kvp.Value.sd;
            if ((now - sd).TotalSeconds > span)
              toDelete.Add(kvp.Key);//mark for deletion
          }

          countDeleted += toDelete.Count;
          foreach (var key in toDelete)
            dict.Remove(key);
        }//lock(dict)
      }

      if (InstrumentationEnabled)
      {
        Interlocked.Add(ref m_stat_CacheRecordCount, countTotal);
        Interlocked.Add(ref m_stat_CacheExpiredRecordCount, countDeleted);
      }
    }

    private void dumpStats()
    {
      var ai = App.Instrumentation;
      if (!this.InstrumentationEnabled || !ai.Enabled) return;

      ai.Record(new Instrumentation.CacheRecordCountGauge(Datum.UNSPECIFIED_SOURCE, Interlocked.Exchange(ref m_stat_CacheRecordCount, 0)));
      ai.Record(new Instrumentation.CacheExpiredRecordCountGauge(Datum.UNSPECIFIED_SOURCE, Interlocked.Exchange(ref m_stat_CacheExpiredRecordCount, 0)));
      ai.Record(new Instrumentation.CacheHitCountGauge(Datum.UNSPECIFIED_SOURCE, Interlocked.Exchange(ref m_stat_CacheHitCount, 0)));
      ai.Record(new Instrumentation.CacheMissCountGauge(Datum.UNSPECIFIED_SOURCE, Interlocked.Exchange(ref m_stat_CacheMissCount, 0)));

      m_stat_RequestCount.SnapshotAllLongsInto<Instrumentation.RequestCountGauge>(ai, 0);
      m_stat_RequestCount.SnapshotAllLongsInto<Instrumentation.ResponseCountGauge>(ai, 0);

      ai.Record(new Instrumentation.ContentFetchLatencyGauge(Datum.UNSPECIFIED_SOURCE, (long)Interlocked.Exchange(ref m_stat_ContentFetchLatencyMs, 0f)));
    }
    #endregion
  }
}
