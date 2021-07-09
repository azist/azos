/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;


namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Represents an instance of managed application process
  /// </summary>
  public class App : ApplicationComponent<GovernorDaemon>, INamed, IOrdered, IComponentDescription
  {
    public const int MAX_TIME_NEVER_CONNECTED_SEC_DEFAULT = 15;
    public const int MAX_TIME_NEVER_CONNECTED_SEC_MIN = 2;
    public const int MAX_TIME_NEVER_CONNECTED_SEC_MAX = 5 * 60;

    public const int MAX_TIME_TORN_SEC_DEFAULT = 12;
    public const int MAX_TIME_TORN_SEC_MIN = 5;
    public const int MAX_TIME_TORN_SEC_MAX = 5 * 60;

    public const int MAX_TIME_INLIMBO_SEC_DEFAULT = 30;
    public const int MAX_TIME_INLIMBO_SEC_MIN = 10;
    public const int MAX_TIME_INLIMBO_SEC_MAX = 5 * 60;

    public const int STOP_TIMEOUT_SEC_DEFAULT = 30;
    public const int STOP_TIMEOUT_SEC_MIN = 3;
    public const int STOP_TIMEOUT_SEC_MAX = 5 * 60;

    public const int START_DELAY_MS_DEFAULT = 500;
    public const int START_DELAY_MS_MIN = 100;
    public const int START_DELAY_MS_MAX = 5 * 60 * 1000;

    public const int STOP_DELAY_MS_DEFAULT = 100;
    public const int STOP_DELAY_MS_MIN = 100;
    public const int STOP_DELAY_MS_MAX = 5 * 60 * 1000;


    public App(GovernorDaemon gov, IConfigSectionNode cfg) : base(gov)
    {
      ConfigAttribute.Apply(this, cfg);
    }

    [Config] private string m_Name;
    [Config] private int m_Order;

    private int m_StartDelayMs = START_DELAY_MS_DEFAULT;
    private int m_StopDelayMs  = STOP_DELAY_MS_DEFAULT;
    private int m_StopTimeoutSec = STOP_TIMEOUT_SEC_DEFAULT;

    private int m_MaxTimeTornSec = MAX_TIME_TORN_SEC_DEFAULT;
    private int m_MaxTimeInLimboSec = MAX_TIME_INLIMBO_SEC_DEFAULT;
    private int m_MaxTimeNeverConnectedSec = MAX_TIME_NEVER_CONNECTED_SEC_DEFAULT;


    private DateTime? m_FailUtc;
    private string m_FailReason;

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_HOST_GOV;

    public string Name => m_Name;
    public int Order => m_Order;

    public string ServiceDescription => "App(`{0}`[{1}])".Args(Name, Order);
    public string StatusDescription
    {
      get
      {
        var result = "Last Start: {0}".Args(LastStartAttemptUtc);
        if (m_FailUtc.HasValue)
        {
          result += "!FAILED! on {0}: {1}".Args(m_FailUtc.Value, m_FailReason);
        }
        if (Connection != null)
        {
          result += "; [{0}]".Args(Connection.State);
          result += "; Strt: {0}".Args(Connection.StartUtc);
          result += "; Recv: {0}".Args(Connection.LastReceiveUtc);
          result += "; Send: {0}".Args(Connection.LastSendUtc);
        }
        return result;
      }
    }



    /// <summary>
    /// True if this application permanently failed.
    /// Once application fails, it can not be corrected.
    /// The system will eventually terminate if there are
    /// any failed apps which are not marked as "Optional"
    /// </summary>
    public bool Failed => m_FailUtc.HasValue;

    /// <summary>
    /// Null or utc timestamp of permanent failure
    /// </summary>
    public DateTime? FailUtc => m_FailUtc;

    /// <summary>
    /// Null or short description of permanent failure
    /// </summary>
    public string FailReason => m_FailReason;

    /// <summary>
    /// Config section for activator to start the process
    /// </summary>
    [Config(path: "start" )]
    public IConfigSectionNode StartSection { get; set; }

    /// <summary>
    /// Config section for activator to stop the process
    /// </summary>
    [Config(path: "stop")]
    public IConfigSectionNode StopSection { get; set; }

    /// <summary>
    /// If true then the system will not stop other applications if this one fails.
    /// False by default, as none of apps are optional by default
    /// </summary>
    [Config(Default = false), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public bool Optional{ get; set; }

    /// <summary>
    /// For how long the app connection may be in a TORN) state, after the expiration the
    /// governor will re-start the application process
    /// </summary>
    [Config(Default = MAX_TIME_INLIMBO_SEC_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int MaxTimeTornSec
    {
      get => m_MaxTimeTornSec;
      set => value.KeepBetween(MAX_TIME_TORN_SEC_MIN, MAX_TIME_TORN_SEC_MAX);
    }

    /// <summary>
    /// For how long the app connection may be in limbo (without getting a ping) state, after the expiration the
    /// governor will re-start the application process
    /// </summary>
    [Config(Default = MAX_TIME_INLIMBO_SEC_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int MaxTimeInLimboSec
    {
      get => m_MaxTimeInLimboSec;
      set => value.KeepBetween(MAX_TIME_INLIMBO_SEC_MIN, MAX_TIME_INLIMBO_SEC_MAX);
    }


    /// <summary>
    /// For how long the app connection may be in limbo (without getting a ping) state, after the expiration the
    /// governor will re-start the application process
    /// </summary>
    [Config(Default = MAX_TIME_NEVER_CONNECTED_SEC_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int MaxTimeNeverConnectedSec
    {
      get => m_MaxTimeNeverConnectedSec;
      set => value.KeepBetween(MAX_TIME_NEVER_CONNECTED_SEC_MIN, MAX_TIME_NEVER_CONNECTED_SEC_MAX);
    }


    /// <summary>
    /// Introduces extra delay before the process start.
    /// The delay may be necessary for some applications to have a chance to fully activate before the next one starts
    /// </summary>
    [Config(Default = START_DELAY_MS_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int StartDelayMs
    {
      get => m_StartDelayMs;
      set => value.KeepBetween(START_DELAY_MS_MIN, START_DELAY_MS_MIN);
    }

    /// <summary>
    /// Introduces extra delay after the process stops.
    /// The delay may be necessary for some applications to have a chance to fully react to this process stop
    /// </summary>
    [Config(Default = STOP_DELAY_MS_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int StopDelayMs
    {
      get => m_StopDelayMs;
      set => value.KeepBetween(STOP_DELAY_MS_MIN, STOP_DELAY_MS_MIN);
    }

    /// <summary>
    /// For how long the app will be awaited for after sending STOP command before the process gets killed
    /// </summary>
    [Config(Default = STOP_TIMEOUT_SEC_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int StopTimeoutSec
    {
      get => m_StopTimeoutSec;
      set => value.KeepBetween(STOP_TIMEOUT_SEC_MIN, STOP_TIMEOUT_SEC_MAX);
    }


    /// <summary>
    /// Assigned by activator
    /// </summary>
    public IAppActivatorContext ActivationContext { get; set; }


    internal ServerAppConnection Connection { get; set;}
    internal DateTime LastStartAttemptUtc { get; set; }


    /// <summary>
    /// Marks this application as permanently failed, the governor will
    /// not try to revive this application.
    /// If the application is not marked as "Optional" then the governor will start termination.
    /// Once application fails, it can not be corrected
    /// </summary>
    /// <param name="reason">Reason status message</param>
    public void Fail(string reason)
    {
      m_FailUtc = App.TimeSource.UTCNow;
      m_FailReason = reason.Default("<Unspecified>");
    }

  }
}
