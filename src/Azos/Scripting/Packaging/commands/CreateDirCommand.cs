/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Serialization.Bix;

namespace Azos.Scripting.Packaging
{
  /// <summary>
  /// Creates directory
  /// </summary>
  [PackageCommand("99eeaaf7-3298-47e8-83bf-0f254a775a77")]
  public sealed class CreateDirCommand : Command
  {
    public string DirectoryName { get; set; }
    protected override void DoExecute(Installer state) => state.CreateDirectory(DirectoryName);

    protected internal override void Deserialize(BixReader reader)
    {
      base.Deserialize(reader);
      DirectoryName = reader.ReadString();
    }

    protected internal override void Serialize(BixWriter writer)
    {
      base.Serialize(writer);
      writer.Write(DirectoryName);
    }
  }

}
