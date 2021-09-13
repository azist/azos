/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Apps
{
  /// <summary>
  /// Defines general-purpose Statuses of runtime areas which can denote different things depending on application.
  /// For example an area may be a zone of a cluster which gets reported as being down or planning to get an upgrade
  /// </summary>
  public enum AreaStatus
  {
    /// <summary>
    /// The status is not defined
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Operates as expected
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Other described normal/expected status
    /// </summary>
    OtherNormal = 2,

    /// <summary>
    /// Other described status
    /// </summary>
    OtherAbnormal = -1,

    /// <summary>
    /// The area is currently failing but may recover within this app lifetime
    /// </summary>
    Failure = -1000,

    /// <summary>
    /// The area has experienced a permanent failure and not likely to recover within this app lifetime
    /// </summary>
    PermanentFailure = -1100,

    /// <summary>
    /// The are is down because it was planned by the system/administrator and is likely to recover within this app lifetime.
    /// For example: turn off for maintenance
    /// </summary>
    PlannedDowntime = -2000,

    /// <summary>
    /// The are is down because it was permanently brought down by the system/administrator.
    /// For example: the system has scaled-down on resources
    /// </summary>
    PermanentDowntime = -2100,

    /// <summary>
    /// The install is about to start
    /// </summary>
    PlannedInstall = -3000,

    /// <summary>
    /// The installation is in progress
    /// </summary>
    Installing = -3100
  }

  /// <summary>
  /// Defines a runtime status of the named area.
  /// Azos application root provides access to IApplicationRealm.Areas
  /// </summary>
  public interface IApplicationRealmArea : Collections.INamed
  {
    /// <summary>
    /// Name of operator reporting status
    /// </summary>
    string Operator { get; }

    /// <summary>
    /// Area/status description
    /// </summary>
    string Description{ get; }

    /// <summary>
    /// Status Code
    /// </summary>
    AreaStatus Status { get; }

    /// <summary>
    /// When was the status reported
    /// </summary>
    DateTime StatusUTC { get; }

    /// <summary>
    /// What is the next status anticipated / if any
    /// </summary>
    AreaStatus NextStatus { get; }

    /// <summary>
    /// When is the next status anticipated / if any
    /// </summary>
    DateTime? NextStatusUTC { get; }
  }


  /// <summary>
  /// Provides status of the surrounding environment (realm) in which application gets executed.
  /// This realm is sub-divided into uniquely-named areas each reporting their status.
  /// This is used by various app components and services to assess the environment status in which they execute, for example:
  /// a logger may suppress error messages from network in a cluster when the area is about to be upgraded to new software.
  /// One may consider this status as a "message board" where services/system check/report the planned or unexpected outages and
  /// adjust their behavior accordingly. Azos provides only the base implementation of such classes delegating the specifics to more
  /// concrete app containers.
  /// </summary>
  public interface IApplicationRealm : IApplicationComponent
  {
    /// <summary>
    /// Registry of named IApplicationRealmArea instances reporting their current status
    /// </summary>
    Collections.IRegistry<IApplicationRealmArea> Areas{ get;}
  }


  /// <summary>
  /// Denotes implementation of IApplicationRealm
  /// </summary>
  public interface IApplicationRealmImplementation : IApplicationRealm, IDisposable
  {
    bool RegisterArea(IApplicationRealmArea area);
    bool UnregisterArea(IApplicationRealmArea area);
  }


  /// <summary>
  /// Provides base implementation for IApplicationRealm
  /// </summary>
  public class ApplicationRealmBase : ApplicationComponent, IApplicationRealmImplementation
  {
    public ApplicationRealmBase(IApplication app) : base(app)
    {
      m_Areas = new Collections.Registry<IApplicationRealmArea>();
    }

    protected Collections.Registry<IApplicationRealmArea> m_Areas;

    public Collections.IRegistry<IApplicationRealmArea> Areas => m_Areas;

    public bool RegisterArea(IApplicationRealmArea area)   => m_Areas.Register(area);
    public bool UnregisterArea(IApplicationRealmArea area) => m_Areas.Unregister(area);

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

  }


  /// <summary>
  /// provides default implementation for IApplicationRealmArea
  /// </summary>
  public struct ApplicationRealmArea : IApplicationRealmArea
  {
    public ApplicationRealmArea(string name,
                                string oper,
                                string descr,
                                AreaStatus status,
                                DateTime utc,
                                AreaStatus nextStatus,
                                DateTime? nextUtc)
    {
      m_Name          =  name;
      m_Operator      =  oper;
      m_Description   =  descr;
      m_Status        =  status;
      m_StatusUTC     =  utc;
      m_NextStatus    =  nextStatus;
      m_NextStatusUTC =  nextUtc;
    }


    private string      m_Name;
    private string      m_Operator;
    private string      m_Description;
    private AreaStatus  m_Status;
    private DateTime    m_StatusUTC;
    private AreaStatus  m_NextStatus;
    private DateTime?   m_NextStatusUTC;

    public string Name             => m_Name;
    public string Operator         => m_Operator;
    public string Description      => m_Description;
    public AreaStatus Status       => m_Status;
    public DateTime StatusUTC      => m_StatusUTC;
    public AreaStatus NextStatus   => m_NextStatus;
    public DateTime? NextStatusUTC => m_NextStatusUTC;
  }

}
