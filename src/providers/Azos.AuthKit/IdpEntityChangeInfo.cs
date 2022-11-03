/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Business;
using Azos.Security;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.AuthKit
{
  /// <summary>
  /// Describes a change of an entity in idp system
  /// </summary>
  [Bix("a5efb46e-ba37-4c5f-9910-0197fa3ffbea")]
  [Schema(Description = "Describes an idp entity change: (EntityId, VersionInfo)")]
  public sealed class IdpEntityChangeInfo : EntityChangeInfo
  {
    [Field(Description = "Gdid portion of .Id")]
    public GDID Id_Gdid => Id.Address.AsGDID(GDID.ZERO);

    [Field(Description = "Entity Type atom of .Id")]
    public Atom Id_Entity => Id.Type;
  }
}
