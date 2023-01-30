/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Collections;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Identifies a locking session
  /// </summary>
  [Serializable]
  public struct LockSessionID : IEquatable<LockSessionID>
  {
    internal LockSessionID(string host)
    {
       Host = host.NonBlank(nameof(host));
       ID = Guid.NewGuid();
    }

    public readonly string Host;
    public readonly Guid ID;

    public bool Equals(LockSessionID other)
    {
      return this.ID.Equals(other.ID) && this.Host.EqualsOrdSenseCase(other.Host);
    }

    public override int GetHashCode()
    {
      return ID.GetHashCode() ^ Host.GetHashCodeOrdSenseCase();
    }

    public override bool Equals(object obj)
    {
      if (obj is LockSessionID) return this.Equals((LockSessionID)obj);
      return false;
    }

    public override string ToString()
    {
      return ID.ToString() + "@" + Host;//faster than Args, perf is critical here as this is sused for registry lokup
    }

    public static bool operator ==(LockSessionID left, LockSessionID right) => left.Equals(right);
    public static bool operator !=(LockSessionID left, LockSessionID right) => !left.Equals(right);
  }


  /// <summary>
  /// Represents a session on a remote lock server under which lock transactions gets executed.
  /// Obtain an instance from local LockManager.MakeSession()
  /// </summary>
  public sealed class LockSession : DisposableObject, INamed, IEquatable<LockSession>
  {
     internal LockSession(ILockManagerImplementation manager, string path, object shardingID, string description = null, int? maxAgeSec = null)
     {
       if (path.IsNullOrWhiteSpace())
          throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+"LockSession.ctor(path==null|empty)");

       if (shardingID==null)
          throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+"LockSession.ctor(shardingID==null)");

       Data = new Server.LockSessionData(new LockSessionID( null ), description, maxAgeSec);
       Manager = manager;
       Path = path;
       ShardingID = shardingID;

       try
       {
         var shardingHash = shardingID.GetHashCode();

         var zone = manager.App.GetMetabase().CatalogReg.NavigateZone(Path);
         var primaryZgovs = zone
                            .FindNearestParentZoneGovernors(iAmZoneGovernor: false, filter: (host) => !host.IsZGovLockFailover,  transcendNOC: true)
                            .OrderBy( host => host.Name)
                            .ToArray();

         if (primaryZgovs.Length<1)
          throw new LockingException(ServerStringConsts.LOCK_SESSION_PATH_LEVEL_NO_ZGOVS_ERROR.Args(Path));

         var failoverZgovs = zone
                             .FindNearestParentZoneGovernors(iAmZoneGovernor: false, filter: (host) => host.IsZGovLockFailover,  transcendNOC: true)
                             .OrderBy( host => host.Name)
                             .ToArray();

         if (failoverZgovs.Length>0 &&
             (primaryZgovs.Length!=failoverZgovs.Length ||
              !primaryZgovs[0].ParentZone.IsLogicallyTheSame( failoverZgovs[0].ParentZone ))
             )
          throw new LockingException(ServerStringConsts.LOCK_SESSION_ZGOV_SETUP_ERROR.Args(primaryZgovs[0].ParentZone.RegionPath));

         var idx = (shardingHash & CoreConsts.ABS_HASH_MASK) % primaryZgovs.Length;
         ServerHostPrimary = primaryZgovs[idx].RegionPath;
         if (failoverZgovs.Length>0)
          ServerHostSecondary = failoverZgovs[idx].RegionPath;


       }
       catch(Exception error)
       {
         throw new LockingException(ServerStringConsts.LOCK_SESSION_PATH_ERROR.Args(Path, error.ToMessageWithType() ) ,error);
       }
     }

     protected override void Destructor()
     {
       Manager.EndLockSession( this );
     }


     internal readonly Server.LockSessionData Data;

     public string Name         { get { return ID.ToString();}}
     public LockSessionID ID    { get { return Data.ID;}}
     public string Description  { get { return Data.Description;}}
     public readonly string Path;
     public readonly string ServerHostPrimary;
     public readonly string ServerHostSecondary;
     public readonly object ShardingID;
     public int? MaxAgeSec      { get { return Data.MaxAgeSec;}}

     /// <summary>
     /// Returns lock server hosts for his session in the primary -> secondary(if any) sequence
     /// </summary>
     public IEnumerable<string> ServerHosts
     {
       get
       {
         yield return ServerHostPrimary;

         if (ServerHostSecondary.IsNotNullOrWhiteSpace())
          yield return ServerHostSecondary;
       }
     }


     /// <summary>
     /// References manager that opened the session
     /// </summary>
     public readonly ILockManagerImplementation Manager;


     public override int GetHashCode()
     {
       return ID.GetHashCode();
     }

     public override bool Equals(object obj)
     {
       var other = obj as LockSession;
       return other!=null?this.Equals(other) : false;
     }

     public bool Equals(LockSession other)
     {
       return other!=null && this.ID.Equals(other.ID);
     }

     public override string ToString()
     {
       return "[{0}]{1}".Args(ID, Description);
     }
  }
}
