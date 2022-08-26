/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Serialization.Bix;

namespace Azos.Scripting.Packaging
{

  /// <summary>
  /// Adds binary data to file being written to
  /// </summary>
  [PackageCommand("c9a899e3-5a93-4b02-a66b-ea4d0c84f0aa")]
  public sealed class FileChunkCommand : Command
  {
    /// <summary>
    /// Where the file chunk starts
    /// </summary>
    public long Offset { get; set; }

    /// <summary>
    /// Raw file chunk data
    /// </summary>
    public byte[] Data { get; set; }

    protected override void DoExecute(Installer state) => state.WriteFileChunk(Offset, Data);

    protected internal override void Serialize(BixWriter writer)
    {
      base.Serialize(writer);
      writer.Write(Offset);
      writer.Write(Data);
    }

    protected internal override void Deserialize(BixReader reader)
    {
      base.Deserialize(reader);
      Offset = reader.ReadLong();
      Data = reader.ReadByteArray();
    }
  }
}
