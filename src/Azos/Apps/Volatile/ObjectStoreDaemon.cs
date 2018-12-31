/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Azos.Conf;
using Azos.Log;
using Azos.Instrumentation;

namespace Azos.Apps.Volatile
{

  /// <summary>
  /// Implements service that stores object in proccess's memory, asynchronously saving objects to external non-volatile storage
  /// upon change and synchronously saving objects upon service stop. This service is useful for scenarios like ASP.NET
  /// volatile domain that can be torn down at any time.
  /// Note for ASP.NET uses: the key difference of this approach from .NET session state management is the fact that this service never blocks
  ///  object CheckIn() operations as backing store is being updated asynchronously.
  /// This class is thread-safe unless specified otherwise on a property/method level
  /// </summary>
  [ConfigMacroContext]
  public class ObjectStoreDaemon : DaemonWithInstrumentation<IApplicationComponent>, IObjectStoreImplementation
  {
    #region CONSTS

        public const int DEFAULT_OBJECT_LEFESPAN_MS = 1 * //hr;
                                                      60 * //min
                                                      60 * //sec
                                                      1000; //msec

        public const int MIN_OBJECT_LIFESPAN_MS = 1000;

        public const string CONFIG_GUID_ATTR = "guid";

        public const string CONFIG_BUCKET_COUNT_ATTR = "bucket-count";
        public const int DEFAULT_BUCKET_COUNT = 1024;
        public const int MAX_BUCKET_COUNT = 0xffff;

        public const string CONFIG_PROVIDER_SECT = "provider";
        public const string CONFIG_OBJECT_LIFE_SPAN_MS_ATTR = "object-life-span-ms";

        public const int MUST_ACQUIRE_INTERVAL_MS = 3000;
    #endregion


    #region .ctor

      /// <summary>
      /// Creates instance of the store service
      /// </summary>
      public ObjectStoreDaemon(IApplication app) : base(app)
      {
      }


      /// <summary>
      /// Creates instance of the store service
      /// </summary>
      public ObjectStoreDaemon(IApplicationComponent director) : base(director)
      {
      }

    #endregion


    #region Private Fields

        private int m_ObjectLifeSpanMS = DEFAULT_OBJECT_LEFESPAN_MS;


        private Guid m_StoreGUID = Guid.NewGuid();

        private ObjectStoreProvider m_Provider;


        private Thread m_Thread;
        private AutoResetEvent m_Trigger = new AutoResetEvent(false);

        private int m_BucketCount = DEFAULT_BUCKET_COUNT;
        private List<Bucket> m_Buckets;

        private bool m_InstrumentationEnabled;

    #endregion


    #region Properties

        public override string ComponentCommonName { get { return "objstore"; }}

        public override string ComponentLogTopic => CoreConsts.OBJSTORE_TOPIC;


        /// <summary>
        /// Returns unique identifier that identifies this particular store.
        /// This ID is used to load store's state from external medium upon start-up.
        /// One may think of this ID as of a "pointer/handle" that survives physical object destroy/create cycle
        /// </summary>
        public Guid StoreGUID
        {
          get { return m_StoreGUID; }

          set
          {
            CheckDaemonInactive();
            m_StoreGUID = value;
          }
        }

        /// <summary>
        /// Implements IInstrumentable
        /// </summary>
        [Config(Default=false)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_OBJSTORE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public override bool InstrumentationEnabled
        {
          get { return m_InstrumentationEnabled;}
          set { m_InstrumentationEnabled = value;}
        }

        /// <summary>
        /// Specifies how many buckets objects are kept in. Increasing this number improves thread concurrency
        /// </summary>
        public int BucketCount
        {
          get { return m_BucketCount;}
          set
          {
            CheckDaemonInactive();
            if (value<1) value = 1;
            if (value>MAX_BUCKET_COUNT) value = MAX_BUCKET_COUNT;
            m_BucketCount = value;
          }
        }


        /// <summary>
        /// Specifies how long objects live without being touched before becoming evicted from the list
        /// </summary>
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_OBJSTORE)]
        public int ObjectLifeSpanMS
        {
          get { return m_ObjectLifeSpanMS; }
          set
          {
            if (value < MIN_OBJECT_LIFESPAN_MS) value = MIN_OBJECT_LIFESPAN_MS;
            m_ObjectLifeSpanMS = value;
          }
        }

        /// <summary>
        /// References provider that persists objects
        /// </summary>
        public ObjectStoreProvider Provider
        {
          get { return m_Provider; }
          set
          {
            CheckDaemonInactive();
            m_Provider = value;
          }
        }

    #endregion


    #region Public



    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted as this method provides logical read-only access. If touch=true then object timestamp is updated
    /// </summary>
    public object Fetch(Guid key, bool touch = false)
    {
      if (this.Status != DaemonStatus.Active) return null;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return null;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return null;
        if (touch)
           entry.LastTime = App.LocalizedTime;
        return entry.Value;
      }
    }



    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted until it is checked back in the store using the same number of calls to CheckIn() for the same GUID.
    /// </summary>
    public object CheckOut(Guid key)
    {
      if (this.Status != DaemonStatus.Active) return null;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return null;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return null;
        entry.Status = ObjectStoreEntryStatus.CheckedOut;
        entry.CheckoutCount++;
        entry.LastTime = App.LocalizedTime;
        return entry.Value;
      }
    }

    /// <summary>
    /// Reverts object state to Normal after the call to Checkout. This way the changes (if any) are not going to be persisted.
    /// Returns true if object was found and checkout canceled. Keep in mind: this method CAN NOT revert inner object state
    ///  to its original state if it was changed, it only unmarks object as changed.
    /// This method is reentrant just like the Checkout is
    /// </summary>
    public bool UndoCheckout(Guid key)
    {
      if (this.Status != DaemonStatus.Active) return false;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return false;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return false;

        if (entry.CheckoutCount>0)
           entry.CheckoutCount--;

        if (entry.CheckoutCount==0)
          entry.Status = ObjectStoreEntryStatus.Normal;
        return true;
      }
    }


    /// <summary>
    /// Puts an object reference "value" into store identified by the "key".
    /// The object is written in the provider when call count to this method equals to CheckOut()
    /// </summary>
    public void CheckIn(Guid key, object value, int msTimeout = 0)
    {
      if (Status != DaemonStatus.Active) return;

      if (value==null)
      {
        Delete(key);
        return;
      }

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;
      bool isnew = false;

      lock (bucket)
      {
        if (!bucket.TryGetValue(key, out entry))
        {
          isnew = true;
          entry = new ObjectStoreEntry();
          entry.Key = key;
          entry.Status = ObjectStoreEntryStatus.ChekedIn;
          entry.LastTime = App.LocalizedTime;
          entry.MsTimeout = msTimeout;
          entry.Value = value;

          bucket.Add(key, entry);
        }
      }

      if (!isnew)
        lock (entry)
        {
          if (entry.Status == ObjectStoreEntryStatus.Deleted) return;

          if (entry.CheckoutCount>0)
           entry.CheckoutCount--;

          if (entry.CheckoutCount==0)
            entry.Status = ObjectStoreEntryStatus.ChekedIn;

          entry.LastTime = App.LocalizedTime;
          entry.MsTimeout = msTimeout;
          entry.Value = value;
        }

    }

    /// <summary>
    /// Puts an object reference "value" into store identified by the "oldKey" under the "newKey".
    /// If oldKey was not checked in, then checks-in under new key as normally would
    /// </summary>
    public void CheckInUnderNewKey(Guid oldKey, Guid newKey, object value, int msTimeout = 0)
    {
      if (Status != DaemonStatus.Active) return;

      if (value==null)
      {
        Delete(oldKey);
        return;
      }

      var bucket = getBucket(oldKey);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(oldKey, out entry)) entry = null;

      if (entry!=null)
        lock (entry)
        {
          entry.Value = null;
          entry.Status = ObjectStoreEntryStatus.Deleted;
          entry.LastTime = App.LocalizedTime;
        }

      CheckIn(newKey, value, msTimeout);
    }


    /// <summary>
    /// Puts an object back into store identified by the "key".
    /// The object is written in the provider when call count to this method equals to CheckOut().
    /// Returns true if object with such id exists and was checked-in
    /// </summary>
    public bool CheckIn(Guid key, int msTimeout = 0)
    {
      if (Status != DaemonStatus.Active) return false;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry))
          return false;

        lock (entry)
        {
          if (entry.Status == ObjectStoreEntryStatus.Deleted) return false;

          if (entry.CheckoutCount>0)
          {
           entry.CheckoutCount--;
           if (entry.CheckoutCount==0)
            entry.Status = ObjectStoreEntryStatus.ChekedIn;

          }
          entry.LastTime = App.LocalizedTime;
          entry.MsTimeout = msTimeout;
        }

      return true;
    }


    /// <summary>
    /// Deletes object identified by key. Returns true when object was found and marked for deletion
    /// </summary>
    public bool Delete(Guid key)
    {
      if (Status != DaemonStatus.Active) return false;


      var bucket = getBucket(key);


      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return false;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return false;
        entry.Status = ObjectStoreEntryStatus.Deleted;
        entry.LastTime = App.LocalizedTime;
      }


      return true;
    }

    #endregion


    #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
          try
          {
            base.DoConfigure(node);

            var sguid = node.AttrByName(CONFIG_GUID_ATTR).ValueAsString();
            if (!string.IsNullOrEmpty(sguid))
              StoreGUID = new Guid(sguid);


            m_Provider = FactoryUtils.MakeAndConfigure<ObjectStoreProvider>(node[CONFIG_PROVIDER_SECT],
                                                                            typeof(FileObjectStoreProvider),
                                                                            new []{ this });

            if (m_Provider == null)
              throw new AzosException("Provider is null");

            ObjectLifeSpanMS = node.AttrByName(CONFIG_OBJECT_LIFE_SPAN_MS_ATTR).ValueAsInt(DEFAULT_OBJECT_LEFESPAN_MS);

            BucketCount = node.AttrByName(CONFIG_BUCKET_COUNT_ATTR).ValueAsInt(DEFAULT_BUCKET_COUNT);

          }
          catch (Exception error)
          {
            throw new AzosException(StringConsts.OBJSTORESVC_PROVIDER_CONFIG_ERROR + error.Message, error);
          }
        }


    protected override void DoStart()
    {
      WriteLog(MessageType.Trace, nameof(DoStart), "Entering");

      try
      {

        //pre-flight checks
        if (m_Provider == null)
          throw new AzosException(StringConsts.DAEMON_INVALID_STATE + "ObjectStoreService.DoStart(Provider=null)");

        m_Provider.Start();

        m_Buckets = new List<Bucket>(m_BucketCount);
        for(var i=0;i<m_BucketCount; i++)
         m_Buckets.Add(new Bucket());

        base.DoStart();

        var clock = Stopwatch.StartNew();
        var now = App.LocalizedTime;
        var all = m_Provider.LoadAll();

        WriteLog(MessageType.Trace, nameof(DoStart), "Prep object list to load in " + clock.Elapsed);

        var cnt = 0;
        foreach (var entry in all)
        {
          entry.Status = ObjectStoreEntryStatus.Normal;
          entry.LastTime = now;

          var bucket = getBucket(entry.Key);
          bucket.Add(entry.Key, entry);

          cnt++;
        }
        WriteLog(MessageType.Trace, nameof(DoStart), "Have loaded {0} objects in {1} ".Args(cnt, clock.Elapsed ));


        m_Thread = new Thread(threadSpin);
        m_Thread.Name = "ObjectStoreDaemon Thread";
        m_Thread.IsBackground = false;

        m_Thread.Start();
      }
      catch (Exception error)
      {
        AbortStart();

        if (m_Thread != null)
        {
          m_Thread.Join();
          m_Thread = null;
        }

        WriteLog(MessageType.CatastrophicError, nameof(DoStart), "Leaked exception: " + error.ToMessageWithType(), error);
        throw error;
      }

      WriteLog(MessageType.Trace, nameof(DoStart), "Exiting");
    }

    protected override void DoSignalStop()
    {
      WriteLog(MessageType.Trace, nameof(DoSignalStop), "Entering");

      try
      {
        base.DoSignalStop();

        m_Trigger.Set();

        //m_Provider should not be touched here
      }
      catch (Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(DoSignalStop), "Leaked: " + error.ToMessageWithType(), error);
        throw error;
      }

      WriteLog(MessageType.Trace, nameof(DoSignalStop), "Exiting");
    }

    protected override void DoWaitForCompleteStop()
    {
      WriteLog(MessageType.Trace, nameof(DoWaitForCompleteStop), "Entering");

      try
      {
        base.DoWaitForCompleteStop();

        m_Thread.Join();
        m_Thread = null;

        m_Provider.WaitForCompleteStop();

        m_Buckets = null;
      }
      catch (Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(DoWaitForCompleteStop), "Leaked exception: " + error.ToMessageWithType(), error);
        throw error;
      }

      WriteLog(MessageType.Trace, nameof(DoWaitForCompleteStop), "Exiting");
    }


    #endregion


    #region .pvt


        private Bucket getBucket(Guid key)
        {
          var idx = (key.GetHashCode() & CoreConsts.ABS_HASH_MASK) % m_BucketCount;
          return m_Buckets[idx];
        }

        private void threadSpin()
        {
          try
          {
              while (Running)
              {
                visit(false);   //todo need to make sure that this visit() never leaks anything otherwise thread may crash the whole service
                m_Trigger.WaitOne(200);
              }//while

              visit(true);
          }
          catch(Exception e)
          {
              WriteLog(MessageType.Emergency, nameof(threadSpin),"Leaked exception: " + e.ToMessageWithType(), e);
          }

          WriteLog(MessageType.Trace, nameof(threadSpin), "Exiting");
        }


        public void visit(bool stopping)
        {
          var now = App.LocalizedTime;
          for(var i=0; i< m_BucketCount; i++)
          {
            var bucket = m_Buckets[i];
            if (stopping || bucket.LastAcquire.AddMilliseconds(MUST_ACQUIRE_INTERVAL_MS)<now)
              lock(bucket) write(bucket);
            else
              if (Monitor.TryEnter(bucket))
              {
                try
                {
                  write(bucket);
                }
                finally
                {
                  Monitor.Exit(bucket);
                }
              }
          }//for
        }


        private void write(Bucket bucket)  //bucket is locked already
        {
          var now = App.LocalizedTime;

          var removed = new Lazy<List<Guid>>(false);

          foreach (var pair in bucket)
          {
            var entry = pair.Value;

            lock(entry)
            {
                //evict expired object and delete evicted or marked for deletion
                if (
                    (
                     (entry.Status == ObjectStoreEntryStatus.Normal  ||
                      entry.Status == ObjectStoreEntryStatus.ChekedIn     //checked-in but not written yet
                     )
                      &&
                     entry.LastTime.AddMilliseconds( (entry.MsTimeout > 0) ? entry.MsTimeout : m_ObjectLifeSpanMS ) < now
                    ) ||
                    (entry.Status == ObjectStoreEntryStatus.Deleted)
                   )
                {
                  var wasWritten = entry.Status == ObjectStoreEntryStatus.Normal || entry.Status == ObjectStoreEntryStatus.Deleted;
                  entry.Status = ObjectStoreEntryStatus.Deleted;//needed for Normal objects that have just expired

                  removed.Value.Add(entry.Key);

                  if (wasWritten)
                  { //delete form disk only if it was already written(normal)
                    try
                    {
                      m_Provider.Delete(entry);
                    }
                    catch (Exception error)
                    {
                      WriteLog(MessageType.CatastrophicError, nameof(write), "Provider error in .Delete(entry): " + error.ToMessageWithType(), error);
                    }
                  }

                  try
                  {
                    if (entry.Value!=null && entry.Value is IDisposable)
                      ((IDisposable)entry.Value).Dispose();
                  }
                  catch (Exception error)
                  {
                    WriteLog(MessageType.Error, nameof(write), "Exception from evicted object IDisposable.Dispose(): " + error.ToMessageWithType(), error);
                  }

                  continue;
                }

                if (entry.Status == ObjectStoreEntryStatus.ChekedIn)
                {

                  try
                  {
                    m_Provider.Write(entry);
                  }
                  catch (Exception error)
                  {
                    WriteLog(MessageType.CatastrophicError, nameof(write),  "Provider error in .Write(entry): " + error.ToMessageWithType(), error);
                  }

                  entry.Status = ObjectStoreEntryStatus.Normal;
                }

            }//lock entry
          }//for


          if (removed.IsValueCreated)
          {
            foreach(var key in removed.Value)
              bucket.Remove(key);

            WriteLog(MessageType.Trace, nameof(write), "Removed {0} objects".Args(removed.Value.Count));
          }

          bucket.LastAcquire = App.LocalizedTime;
        }

    #endregion

  }
}
