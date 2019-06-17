/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


using Azos.Apps;
using Azos.Conf;
using Azos.IO;
using Azos.Serialization.Slim;


namespace Azos.Pile
{
  /// <summary>
  /// Provides default implementation of IPile which stores objects in Memory Mapped Files
  /// </summary>
  [SlimSerializationProhibited]
  public sealed class MMFPile : DefaultPileBase
  {
    #region .ctor

    public MMFPile(IApplication app, string name = null) : base(app, name)
    {
    }

    public MMFPile(IApplicationComponent director, string name = null) : base(director, name)
    {
    }

    #endregion


    private string m_DataDirectoryRoot;
    private Azos.Time.Event m_ManagerEvent;


    #region Properties


      /// <summary>
      /// Specifies the full path to directory root under which the MMFPile creates a named instance directory where the memory-mapped files are kept.
      /// The instance directory name is taken from Pile.Name
      /// </summary>
      [Config]
      public string DataDirectoryRoot
      {
        get{ return m_DataDirectoryRoot ?? string.Empty;}
        set
        {
          CheckDaemonInactive();
          m_DataDirectoryRoot = value;
        }
      }

      /// <summary>
      /// Returns the full path root + name where the memory mapped files are kept
      /// </summary>
      public string DataDirectory
      {
        get{ return Path.Combine(DataDirectoryRoot, Name);}
      }

      /// <summary>
      /// Returns PilePersistence.Memory
      /// </summary>
      public override ObjectPersistence Persistence { get{ return ObjectPersistence.MemoryDisk; }}

      /// <summary>
      /// Returns true when pile has completely loaded - mounted and Crawled(), from the MMF images on disk
      /// </summary>
      public bool CompletelyLoaded
      {
        get
        {
          return __getSegmentsAtStart().All( s => s==null || s.LOADED_AND_CRAWLED );
        }
      }

    #endregion

    #region Protected

      protected override void DoStart()
      {
        if (!Directory.Exists(DataDirectoryRoot))
          throw new PileException(StringConsts.PILE_MMF_NO_DATA_DIRECTORY_ROOT_ERROR.Args(DataDirectoryRoot));

        Directory.CreateDirectory(DataDirectory);

        readCurrentTypeRegistry();

        base.DoStart();

        //load existing data
        var segFiles = MMFMemory.GetSegmentFileNames(DataDirectory);

        var pidx = -1;
        for(var i=0; i<segFiles.Length; i++)
        {
          var segFile = segFiles[i];
          for(var j=pidx+1; j<segFile.Value; j++)
            __addSegmentAtStart( null );
          pidx = segFile.Value;

          var memory = new MMFMemory(DataDirectory, segFile.Value);
          var segment = new DefaultPileBase._segment(this, memory, false);
          __addSegmentAtStart(segment);
        }

        //1 thread since this is a io-bound operation
        var segments = __getSegmentsAtStart();
        Task.Delay(500).ContinueWith( (a) => crawlSegmentsAtStart(segments), TaskContinuationOptions.LongRunning);

        m_ManagerEvent = new Azos.Time.Event(App.EventTimer,
                                            interval: new TimeSpan(0, 0, 20),
                                            enabled: true,
                                            body: _ => AcceptManagerVisit(this, DateTime.UtcNow),
                                            bodyAsyncModel: Time.EventBodyAsyncModel.AsyncTask){};
      }

      protected override void DoWaitForCompleteStop()
      {
        DisposeAndNull(ref m_ManagerEvent);
        writeCurrentTypeRegistry();
        base.DoWaitForCompleteStop();
      }

      /// <summary>
      /// Creates a segment that stores data in memory mapped files
      /// </summary>
      internal override DefaultPileBase._segment MakeSegment(int segmentNumber)
      {
        var memory = new MMFMemory(DataDirectory, segmentNumber, SegmentSize);
        var result = new DefaultPileBase._segment(this, memory, true);
        return result;
      }

      protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
      {
        base.DoAcceptManagerVisit(manager, managerNow);

        try
        {
          writeCurrentTypeRegistry();
        }
        catch(Exception error)
        {
          App.Log.Write(new Log.Message
            {
              Type = Log.MessageType.Error,
              Source = 2000,
              Topic = CoreConsts.APPLICATION_TOPIC,
              From = "MMFPile.DoAcceptManagerVisit()",
              Text = "MMFPile Exception leaked from writeCurrentTypeRegistry(): {0}".Args(error.ToMessageWithType()),
              Exception = error
            });
        }
      }

    #endregion

    #region .pvt

      private void crawlSegmentsAtStart(_segment[] segments)
      {
        try
        {
          crawlSegmentsAtStartCore(segments);
        }
        catch(Exception error)
        {
          App.Log.Write(new Log.Message
            {
              Type = Log.MessageType.CatastrophicError,
              Source = 1000,
              Topic = CoreConsts.APPLICATION_TOPIC,
              From = "MMFPile.crawlSegmentsAtStart()",
              Text = "MMFPile crawl on start failed. Exception leaked: {0}".Args(error.ToMessageWithType()),
              Exception = error
            });
        }
      }

      private void crawlSegmentsAtStartCore(_segment[] segments)
      {
        for(var i=0; i<segments.Length; i++)
        {
          var segment = segments[i];
          if (Thread.VolatileRead(ref segment.DELETED)!=0) continue;
          if (segment.LOADED_AND_CRAWLED) continue;

          if (!getWriteLock(segment)) return;//service shutting down, no need to continue
          try
          {
             if (Thread.VolatileRead(ref segment.DELETED)!=0 || segment.Disposed) continue;
             //crawl
             var status = segment.Crawl();
             //assign status
             segment.ObjectCount = (int)status.ObjectCount;
             segment.ObjectLinkCount = (int)status.ObjectLinkCount;
             segment.UsedBytes = (int)status.UsedPayloadSize;
             segment.LOADED_AND_CRAWLED = true;//ready to be used
          }
          catch(Exception error)
          {
            App.Log.Write(new Log.Message
            {
              Type = Log.MessageType.Critical,
              Source = 2000,
              Topic = CoreConsts.APPLICATION_TOPIC,
              From = "MMFPile.crawlSegmentsAtStartCore()",
              Text = "Exception leaked from segment[{0}].Crawl(): {1}".Args(i, error.ToMessageWithType()),
              Exception = error
            });
          }
          finally
          {
            releaseWriteLock(segment);
          }
        }
      }

      const string TREG_FILE = "pile-typereg";

      private void readCurrentTypeRegistry()
      {
        var fn = Path.Combine(DataDirectory, TREG_FILE);
        if (!File.Exists(fn)) return;

        using(var fs = new FileStream(fn, FileMode.Open, FileAccess.Read))
        {
          var ser = new SlimSerializer();
          var tr = ser.Deserialize(fs) as TypeRegistry;

          m_CurrentTypeRegistry = tr;
        }
      }

      private void writeCurrentTypeRegistry()
      {
        var fn = Path.Combine(DataDirectory, TREG_FILE);

        using(var fs = new FileStream(fn, FileMode.Create, FileAccess.Write))
        {
          var ser = new SlimSerializer();
          ser.Serialize(fs, m_CurrentTypeRegistry);
        }
      }

    #endregion

  }


}
