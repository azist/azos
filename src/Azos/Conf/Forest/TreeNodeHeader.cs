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
using Azos.Serialization.Bix;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Provides minimal tree node information used for listing
  /// </summary>
  [Bix("a897b1af-b2a9-463a-a361-c307ba10e779")]
  [Schema(immutable: true, Description = "Provides minimal tree node information used for listing")]
  public sealed class TreeNodeHeader : TransientModel
  {
    [Field(required: true, Description = "EntityId for listed node")]
    public EntityId Id { get; set; }

    [Field(required: true, Description = "Gdid of this data version")]
    public GDID G_Version { get; set; }

    [Field(required: true, Description = "Path segment of the parent")]
    public string PathSegment { get; set; }

    [Field(required: true, Description = "Timestamp as of which this tree node becomes effective")]
    public DateTime StartUtc { get; set; }
  }
}
