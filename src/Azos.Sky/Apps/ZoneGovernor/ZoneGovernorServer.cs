/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Apps.Injection;

namespace Azos.Sky.Apps.ZoneGovernor
{
  /// <summary>
  /// Implements contracts trampoline that uses a singleton instance of ZoneGovernorService
  /// </summary>
  public sealed class ZoneGovernorServer : Azos.Sky.Contracts.IZoneTelemetryReceiver,
                                           Azos.Sky.Contracts.IZoneLogReceiver,
                                           Azos.Sky.Contracts.IZoneHostRegistry,
                                           Azos.Sky.Contracts.IZoneHostReplicator,
                                           Azos.Sky.Contracts.ILocker
  {
    [Inject] IApplication m_App;
    public ZoneGovernorService Service => m_App.NonNull(nameof(m_App))
                                              .Singletons
                                              .Get<ZoneGovernorService>() ?? throw new AZGOVException(StringConsts.AZGOV_INSTANCE_NOT_ALLOCATED_ERROR);


    public int SendTelemetry(string host, Azos.Instrumentation.Datum[] data)
      => Service.SendTelemetry(host, data);

    public int SendLog(string host, string appName, Azos.Log.Message[] data)
      => Service.SendLog(host, appName, data);

    public Contracts.HostInfo GetSubordinateHost(string hostName)
      => Service.GetSubordinateHost(hostName);

    public IEnumerable<Contracts.HostInfo> GetSubordinateHosts(string hostNameSearchPattern)
      => Service.GetSubordinateHosts(hostNameSearchPattern);

    public void RegisterSubordinateHost(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
      => Service.RegisterSubordinateHost(host, hid);

    public Contracts.DynamicHostID Spawn(string hostPath, string id = null)
      => Service.Spawn(hostPath, id);

    public void PostDynamicHostInfo(Contracts.DynamicHostID hid, DateTime stamp, string owner, int votes)
      => Service.PostDynamicHostInfo(hid, stamp, owner, votes);

    public void PostHostInfo(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
      => Service.PostHostInfo(host, hid);

    public Contracts.DynamicHostInfo GetDynamicHostInfo(Contracts.DynamicHostID hid)
      => Service.GetDynamicHostInfo(hid);

    public Locking.LockTransactionResult ExecuteLockTransaction(Locking.Server.LockSessionData session, Locking.LockTransaction transaction)
      => Service.Locker.ExecuteLockTransaction(session, transaction);

    public bool EndLockSession(Locking.LockSessionID sessionID)
      => Service.Locker.EndLockSession(sessionID);
  }
}
