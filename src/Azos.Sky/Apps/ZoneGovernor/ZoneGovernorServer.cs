/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;


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
    public int SendTelemetry(string host, Azos.Instrumentation.Datum[] data)
    {
      return ZoneGovernorService.Instance.SendTelemetry(host, data);
    }

    public int SendLog(string host, string appName, Azos.Log.Message[] data)
    {
      return ZoneGovernorService.Instance.SendLog(host, appName, data);
    }

    public Contracts.HostInfo GetSubordinateHost(string hostName)
    {
      return ZoneGovernorService.Instance.GetSubordinateHost(hostName);
    }

    public IEnumerable<Contracts.HostInfo> GetSubordinateHosts(string hostNameSearchPattern)
    {
      return ZoneGovernorService.Instance.GetSubordinateHosts(hostNameSearchPattern);
    }

    public void RegisterSubordinateHost(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
    {
      ZoneGovernorService.Instance.RegisterSubordinateHost(host, hid);
    }

    public Contracts.DynamicHostID Spawn(string hostPath, string id = null)
    {
      return ZoneGovernorService.Instance.Spawn(hostPath, id);
    }

    public void PostDynamicHostInfo(Contracts.DynamicHostID hid, DateTime stamp, string owner, int votes)
    {
      ZoneGovernorService.Instance.PostDynamicHostInfo(hid, stamp, owner, votes);
    }

    public void PostHostInfo(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
    {
      ZoneGovernorService.Instance.PostHostInfo(host, hid);
    }

    public Contracts.DynamicHostInfo GetDynamicHostInfo(Contracts.DynamicHostID hid)
    {
      return ZoneGovernorService.Instance.GetDynamicHostInfo(hid);
    }

    public Locking.LockTransactionResult ExecuteLockTransaction(Locking.Server.LockSessionData session, Locking.LockTransaction transaction)
    {
      return ZoneGovernorService.Instance.Locker.ExecuteLockTransaction(session, transaction);
    }

    public bool EndLockSession(Locking.LockSessionID sessionID)
    {
      return ZoneGovernorService.Instance.Locker.EndLockSession(sessionID);
    }
  }
}
