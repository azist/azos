/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;


namespace Azos.Conf.Forest
{
  /// <summary>
  /// Describes a change of an tree node
  /// </summary>
  [Bix("a6773e08-d835-4c33-9b98-e073fb1eb872")]
  [Schema(Description = "Describes a tree node change: (EntityId, VersionInfo)")]
  public sealed class TreeNodeChangeInfo : EntityChangeInfo
  {
    [Field(Description = "Gdid portion of .Id")]
    public GDID Id_Gdid => Id.Address.AsGDID(GDID.ZERO);

    [Field(Description = "Entity Type atom of .Id")]
    public Atom Id_Entity => Id.Type;
  }
}
