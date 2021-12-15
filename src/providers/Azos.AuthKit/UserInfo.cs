/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Data.Business;
using Azos.Security;
using Azos.Serialization.Bix;

namespace Azos.AuthKit
{
  /// <summary>
  /// Describes user account
  /// </summary>
  [Bix("3485e189-106e-449f-b34e-892a74a01ec7")]
  [Schema(Description = "Describes user account")]
  public sealed class UserInfo : TransientModel
  {
    /// <summary>
    /// User account realm
    /// </summary>
    [Field(required: true, Description = "User account realm")]
    public Atom Realm { get; set; }

    /// <summary>
    /// User account Gdid
    /// </summary>
    [Field(required: true, Description = "User account Gdid")]
    public GDID Gdid { get; set; }

    /// <summary>
    /// User account Gdid
    /// </summary>
    [Field(required: true, Description = "User account Guid")]
    public Guid Guid { get; set; }

    /// <summary>
    /// Name/Title of user account
    /// </summary>
    [Field(required: true, Description = "Name/Title of user account")]
    public string Name { get; set; }

    /// <summary>
    /// User access level
    /// </summary>
    [Field(required: true, Description = "User access level")]
    public UserStatus Level { get; set; }

    /// <summary>
    /// User description
    /// </summary>
    [Field(required: true, Description = "User description")]
    public string Description { get; set; }


  }
}
