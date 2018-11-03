using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Instrumentation;

namespace Azos.Sky.Workers.Server.Queue
{
  /// <summary>
  /// Represents a Named Queue instance
  /// </summary>
  public sealed class TodoQueue : ApplicationComponent, INamed, IInstrumentable
  {
    /// <summary>
    /// Denotes mode of execution: sequential, parallel or ParallelByKey
    /// </summary>
    public enum ExecuteMode
    {
      Sequential = 0,
      Parallel,
      ParallelByKey
    }

    /// <summary>
    /// Defines how duplicate todo submissions are handled by the queue.
    /// When a client sends a Todo into server, a server may accept the todo but client may not get the confirmation message,
    /// in which case client may re-submit the same todo (as identified by the SysID GDID) more than once.
    /// This leads to duplication of work and may be unacceptable for some applications (e.g. financial double-posting).
    /// Duplication detection will respond with OK acknowledgement for the Todo that have already been accepted.
    /// </summary>
    public enum DuplicationHandlingMode
    {
      /// <summary>
      /// The server is not checking for duplicate Todo instance submission.
      /// This is the fastest mode but leads to possible re-submission of the same Todo instance
      /// </summary>
      NotDetected = -10,

      /// <summary>
      /// This is the default mode.
      /// The server is going to check whether a Todo with the same SysID was already submitted recently.
      /// The recency (time window size) is defined by the server configuration and is usually configured
      /// to represent a typical timeouts (60 sec = 3 typical 20 sec Glue calls) for re-submission.
      /// This detection mode is usually done in-RAM and provides the best balance between performance and protection guarantees.
      /// Note: this mode DOES NOT detect a case when this server was down and traffic was routed to a different server in a hostset.
      /// For more protection use HostsetDetection mode
      /// </summary>
      HostFastDetection = 0,

      /// <summary>
      /// The server is going to check whether a Todo with the same SysID was already submitted and is in the queue.
      /// This detection mode is usually done in-store and provides the best host-level protection but takes more time to execute
      /// a store check request. This mode does not detect todos which have executed already and not in either queue or recent buffer
      /// </summary>
      HostAccurateDetection = 10,


      /// <summary>
      /// The server will check itself then others in a host set for possible Todo resubmission. This mode provides the best protection
      /// at the expense of performance as cross-checking introduces extra network traffic.
      /// </summary>
      HostsetDetection = 20
    }



    #region CONSTS

      public const int DEFAULT_BATCH_SIZE = 32;

      public const int MAX_BATCH_SIZE   = 1024;

      public const int DEFAULT_ACQUIRE_TIMEOUT_SEC = 7 * 60;
      public const int MIN_ACQUIRE_TIMEOUT_SEC = 60;

    #endregion

    #region .ctor

      internal TodoQueue(TodoQueueService director, IConfigSectionNode node) : base(director)
      {
        ConfigAttribute.Apply(this, node);

        if (Name.IsNullOrWhiteSpace())
          throw new WorkersException(GetType().Name + ".ctor($name=null|empty)");
      }

    #endregion

    #region Fields

      #pragma warning disable CS0649
      [Config]
      private string m_Name;

      private int m_BatchSize = DEFAULT_BATCH_SIZE;


      [Config]
      private DuplicationHandlingMode m_DuplicationHandling;


      [Config]
      private ExecuteMode m_Mode;
      #pragma warning restore CS0649

      private int m_AcquireTimeoutSec = DEFAULT_ACQUIRE_TIMEOUT_SEC;

      private object m_AcquireLock = new object();
      private DateTime? m_AcquireDateUTC;

    #endregion

    #region Properties

      /// <summary>
      /// Unique queue name
      /// </summary>
      public string Name { get { return m_Name;} }

      /// <summary>
      /// References service that this queue is under
      /// </summary>
      public TodoQueueService QueueService { get { return (TodoQueueService)ComponentDirector;} }

      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public bool InstrumentationEnabled { get; set; }


      /// <summary>
      /// Specifies the size of one processing quantum in todo instance count
      /// </summary>
      [Config(Default = DEFAULT_BATCH_SIZE)]
      public int BatchSize
      {
         get {return m_BatchSize;}
         private set
         {
           m_BatchSize = value<1 ? 1 : value > MAX_BATCH_SIZE ? MAX_BATCH_SIZE : value;
         }
      }

      /// <summary>
      /// Specifies the timeout for queue acquisition.
      /// The queue is acquired every time a processing quantum starts.
      /// If processing stalls for some reason (i.e. long sequential todo) then
      /// queue is acquired anyway after timeout expires.
      /// The timeout must work in conjunction with BtachSize to ensure that during normal operation it never happens, because
      /// sequential processing order is not guaranteed when queue times-out
      /// </summary>
      [Config(Default = DEFAULT_ACQUIRE_TIMEOUT_SEC)]
      public int AcquireTimeoutSec
      {
         get {return m_AcquireTimeoutSec;}
         private set
         {
           m_AcquireTimeoutSec = value<MIN_ACQUIRE_TIMEOUT_SEC ? MIN_ACQUIRE_TIMEOUT_SEC : value;
         }
      }

      /// <summary>
      /// Denotes mode of execution: sequential, parallel or ParallelByKey
      /// </summary>
      public ExecuteMode Mode { get { return m_Mode;} }

      /// <summary>
      /// True when queue is acquired by processing quanta
      /// </summary>
      public bool Acquired { get {  return AcquireDateUTC.HasValue; } }

      /// <summary>
      /// When was queue acquired for the last time - used for timeout
      /// </summary>
      public DateTime? AcquireDateUTC
      {
        get
        {
          lock(m_AcquireLock) return m_AcquireDateUTC;
        }
      }

      /// <summary>
      /// Specifies how this queue handles duplicate Todo submissions
      /// </summary>
      public DuplicationHandlingMode DuplicationHandling { get { return m_DuplicationHandling; } }

    #endregion

    #region Public/Internal


      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      {
        return ExternalParameterAttribute.GetParameters(this, groups);
      }

      /// <summary>
      /// Gets external parameter value returning true if parameter was found
      /// </summary>
      public bool ExternalGetParameter(string name, out object value, params string[] groups)
      {
          return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
      }

      /// <summary>
      /// Sets external parameter value returning true if parameter was found and set
      /// </summary>
      public bool ExternalSetParameter(string name, object value, params string[] groups)
      {
        return ExternalParameterAttribute.SetParameter(this, name, value, groups);
      }

      internal bool CanBeAcquired(DateTime utcNow)
      {
        lock(m_AcquireLock)
        {
          if (m_AcquireDateUTC.HasValue && (utcNow - m_AcquireDateUTC.Value).TotalSeconds < m_AcquireTimeoutSec) return false;
          return true;
        }
      }

      internal bool TryAcquire(DateTime utcNow)
      {
        lock(m_AcquireLock)
        {
          if (m_AcquireDateUTC.HasValue && (utcNow - m_AcquireDateUTC.Value).TotalSeconds < m_AcquireTimeoutSec) return false;
          m_AcquireDateUTC = utcNow;
          return true;
        }
      }

      internal bool Release()
      {
        lock(m_AcquireLock)
        {
          var result = m_AcquireDateUTC.HasValue;
          m_AcquireDateUTC = null;
          return result;
        }
      }


      public override string ToString()
      {
        return "Queue('{0}')".Args(Name);
      }
    #endregion
  }
}
