/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Serialization.Bix;
using System;

namespace Azos.Scripting.Packaging
{
  /// <summary>
  /// Changes current directory to new subdir, goes to root with '/' or level up with '..'
  /// </summary>
  [PackageCommand("bb164465-bc8c-4d08-9830-bc1418fccdbf")]
  public sealed class ChangeDirCommand : Command
  {
    public string Path { get; set; }
    protected override void DoExecute(Installer state) => state.ChangeDirectory(Path);

    protected internal override void Deserialize(BixReader reader)
    {
      base.Deserialize(reader);
      Path = reader.ReadString();
    }

    protected internal override void Serialize(BixWriter writer)
    {
      base.Serialize(writer);
      writer.Write(Path);
    }
  }

}
