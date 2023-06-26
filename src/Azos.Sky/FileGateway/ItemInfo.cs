/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.Sky.FileGateway
{
  [Bix("595824e8-da4a-4117-b0d8-e88b8a5b6e59")]
  [Schema(Description = "Provides information of files or directories")]
  public sealed class ItemInfo : TransientModel
  {
    [Field(Required = true, Description = "Full item path")]
    public EntityId Path{ get; set; }

    [Field(Required = true, Description = "A file or directory")]
    public ItemType Type { get; set; }

    [Field(Required = true, Description = "When item was created")]
    public DateTime CreateUtc{ get; set; }

    [Field(Required = true, Description = "When item was changed for the last time")]
    public DateTime LastChangeUtc { get; set; }

    [Field(Required = true, Description = "Items size in bytes or item numbers (such as directories)")]
    public long Size { get; set; }
  }
}
