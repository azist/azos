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
  ///  Creates file
  /// </summary>
  [PackageCommand("b95712f5-9a69-49e5-b848-999b206e815f")]
  public sealed class CreateFileCommand : Command
  {
    public string FileName { get; set; }
    protected override void DoExecute(Installer state) => state.CreateFile(FileName);

    protected internal override void Deserialize(BixReader reader)
    {
      base.Deserialize(reader);
      FileName = reader.ReadString();
    }

    protected internal override void Serialize(BixWriter writer)
    {
      base.Serialize(writer);
      writer.Write(FileName);
    }
  }

}
