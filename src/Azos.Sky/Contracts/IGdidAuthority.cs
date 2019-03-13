/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Glue;
using Azos.Data;

using Azos.Sky.Security.Permissions.Admin;
using System.IO;

namespace Azos.Sky.Contracts
{
  /// <summary>
  /// Represents a Global Distributed ID Authority contract
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IGdidAuthority : ISkyService
  {
    GdidBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = GDID.COUNTER_MAX);
  }

  /// <summary>
  /// Contract for client for IGDIDAuthority service
  /// </summary>
  public interface IGdidAuthorityClient : ISkyServiceClient, IGdidAuthority
  {
    CallSlot Async_AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = GDID.COUNTER_MAX);
  }


  /// <summary>
  /// Provides Global Distributed ID block allocated by authority
  /// </summary>
  [Serializable]
  public sealed class GdidBlock : TypedDoc
  {
    [Field]public string   ScopeName    { get; internal set;}
    [Field]public string   SequenceName { get; internal set;}
    [Field]public int      Authority    { get; internal set;}
    [Field]public string   AuthorityHost{ get; internal set;}
    [Field]public uint     Era          { get; internal set;}
    [Field]public ulong    StartCounterInclusive { get; internal set;}
    [Field]public int      BlockSize     { get; internal set;}
    [Field]public DateTime ServerUTCTime { get; internal set;}

    [NonSerialized]
    internal int __Remaining;//how much left per block
  }


  /// <summary>
  /// Represents a backup location where GDID Authority persists its data
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IGdidPersistenceRemoteLocation : ISkyService
  {
      GDID? Read(byte authority, string sequenceName, string scopeName);
      void Write(string sequenceName, string scopeName, GDID value);
  }

  /// <summary>
  /// Contract for client for IGdidPersistenceRemoteLocation service
  /// </summary>
  public interface IGdidPersistenceRemoteLocationClient : ISkyServiceClient, IGdidPersistenceRemoteLocation
  {

  }
}
