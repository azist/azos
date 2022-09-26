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
  ///  Changes file access mode
  /// </summary>
  [PackageCommand("32b42351-5c47-4ec7-b859-8318afc0fc0b")]
  public sealed class ChmodCommand : Command
  {
    public string FileName { get; set; }
    public bool? CanRead { get; set; }
    public bool? CanWrite { get; set; }
    public bool? CanExecute { get; set; }
    protected override void DoExecute(Installer state) => state.Chmod(FileName, CanRead, CanWrite, CanExecute);

    protected internal override void Deserialize(BixReader reader)
    {
      base.Deserialize(reader);
      FileName = reader.ReadString();
      CanRead = reader.ReadNullableBool();
      CanWrite = reader.ReadNullableBool();
      CanExecute = reader.ReadNullableBool();
    }

    protected internal override void Serialize(BixWriter writer)
    {
      base.Serialize(writer);
      writer.Write(FileName);
      writer.Write(CanRead);
      writer.Write(CanWrite);
      writer.Write(CanExecute);
    }
  }

}
