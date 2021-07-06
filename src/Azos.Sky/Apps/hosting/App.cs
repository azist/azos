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
  public class App : ApplicationComponent<GovernorDaemon>, INamed, IOrdered
  {
    public const int MAX_TIME_TORN_SEC_DEFAULT = 60;
    public const int MAX_TIME_INLIMBO_SEC_DEFAULT = 120;

    public const int STOP_TIMEOUT_SEC_DEFAULT = 30;
    public const int STOP_TIMEOUT_SEC_MIN = 3;
    public const int STOP_TIMEOUT_SEC_MAX = 5 * 60;

    public App(GovernorDaemon gov, IConfigSectionNode cfg) : base(gov)
    {
      ConfigAttribute.Apply(this, cfg);
    }

    [Config] private string m_Name;
    [Config] private int m_Order;

    private int m_StopTimeoutSec = STOP_TIMEOUT_SEC_DEFAULT;


    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_HOST_GOV;

    public string Name => m_Name;
    public int Order => m_Order;


    /// <summary>
    /// Config section for activator to start the process
    /// </summary>
    [Config(path: "start" )]
    public IConfigSectionNode StartSection { get; set; }

    /// <summary>
    /// Config section for activator to stop the process
    /// </summary>
    [Config(path: "stop")]
    private IConfigSectionNode StopSection { get; set; }

    /// <summary>
    /// For how long the app connection may be in torn state, after the expiration the
    /// governor will re-start the application process
    /// </summary>
    [Config(Default = MAX_TIME_TORN_SEC_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    private int MaxTimeTornSec { get; set; } = MAX_TIME_TORN_SEC_DEFAULT;

    /// <summary>
    /// For how long the app connection may be in limbo (without getting a ping) state, after the expiration the
    /// governor will re-start the application process
    /// </summary>
    [Config(Default = MAX_TIME_INLIMBO_SEC_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    private int MaxTimeInLimboSec { get; set; } = MAX_TIME_INLIMBO_SEC_DEFAULT;

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

  }
}
