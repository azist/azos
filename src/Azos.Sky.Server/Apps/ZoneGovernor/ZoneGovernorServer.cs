/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Apps.Injection;
using Azos.Sky.Contracts;
using Azos.Sky.Locking;
using Azos.Sky.Locking.Server;

namespace Azos.Apps.ZoneGovernor
{
  /// <summary>
  /// Implements contracts trampoline that uses a singleton instance of ZoneGovernorService
  /// </summary>
  public sealed class ZoneGovernorServer : IZoneTelemetryReceiver,
                                           IZoneLogReceiver,
                                           IZoneHostRegistry,
                                           IZoneHostReplicator,
                                           ILocker
  {
    [Inject] IApplication m_App;

    public ZoneGovernorService Service => m_App.NonNull(nameof(m_App))
                                              .Singletons
                                              .Get<ZoneGovernorService>() ?? throw new AZGOVException(Sky.ServerStringConsts.AZGOV_INSTANCE_NOT_ALLOCATED_ERROR);


    public int SendTelemetry(string host, Azos.Instrumentation.Datum[] data)
      => Service.SendTelemetry(host, data);

    public int SendLog(string host, string appName, Azos.Log.Message[] data)
      => Service.SendLog(host, appName, data);

    public HostInfo GetSubordinateHost(string hostName)
      => Service.GetSubordinateHost(hostName);

    public IEnumerable<HostInfo> GetSubordinateHosts(string hostNameSearchPattern)
      => Service.GetSubordinateHosts(hostNameSearchPattern);

    public void RegisterSubordinateHost(HostInfo host, DynamicHostID? hid)
      => Service.RegisterSubordinateHost(host, hid);

    public DynamicHostID Spawn(string hostPath, string id = null)
      => Service.Spawn(hostPath, id);

    public void PostDynamicHostInfo(DynamicHostID hid, DateTime stamp, string owner, int votes)
      => Service.PostDynamicHostInfo(hid, stamp, owner, votes);

    public void PostHostInfo(HostInfo host, DynamicHostID? hid)
      => Service.PostHostInfo(host, hid);

    public DynamicHostInfo GetDynamicHostInfo(DynamicHostID hid)
      => Service.GetDynamicHostInfo(hid);

    public LockTransactionResult ExecuteLockTransaction(LockSessionData session, LockTransaction transaction)
      => Service.Locker.ExecuteLockTransaction(session, transaction);

    public bool EndLockSession(LockSessionID sessionID)
      => Service.Locker.EndLockSession(sessionID);
  }
}
