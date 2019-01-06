/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Collections;

namespace Azos.Sky.Locking.Server
{
  /// <summary>
  /// Contains data to establish a session on a remote lock server.
  /// Clients should use LockSession instead, which is obtained from the local LockManager.MakeSession
  /// </summary>
  [Serializable]
  public sealed class LockSessionData : INamed,  IEquatable<LockSessionData>
  {
     internal LockSessionData(LockSessionID id, string description, int? maxAgeSec)
     {
       ID = id;
       Description = description;
       MaxAgeSec = maxAgeSec;
     }

     public string Name { get { return ID.ToString();}}
     public readonly LockSessionID ID;
     public readonly string Description;
     public readonly int? MaxAgeSec;

     public override int GetHashCode()
     {
       return ID.GetHashCode();
     }

     public override bool Equals(object obj)
     {
       var other = obj as LockSessionData;
       return other!=null?this.Equals(other) : false;
     }

     public bool Equals(LockSessionData other)
     {
       return other!=null && this.ID.Equals(other.ID);
     }

     public override string ToString()
     {
       return "[{0}]{1}".Args(ID, Description);
     }
  }



        internal sealed class ServerLockSession : DisposableObject, INamed
        {
           internal ServerLockSession(LockSessionData data)
           {
             Data = data;
           }

           protected override void Destructor()
           {
             foreach( var ns in m_MutatedNamespaces)
              lock(ns)
              {
                foreach(var tbl in ns.Tables)
                 tbl.EndSession( this );
              }
           }

           internal HashSet<Namespace> m_MutatedNamespaces = new HashSet<Namespace>();



           public readonly LockSessionData Data;

           internal DateTime m_LastInteractionUTC;

           public string Name { get { return Data.Name; } }

           public LockSessionID ID { get{ return Data.ID;}}

           public override int GetHashCode() { return Data.GetHashCode(); }

           public override bool Equals(object obj) { return Data.Equals(((ServerLockSession)obj).Data); }
        }




}
