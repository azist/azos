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
  /// Executes script on target OS system
  /// </summary>
  [PackageCommand("90b71e1c-8aa8-422d-8213-c19414de845f")]
  public sealed class ExecOsCommand : Command
  {
    public string CommandText{ get; set; }

    protected override void DoExecute(Installer state) => state.ExecuteOsScript(CommandText);

    protected internal override void Deserialize(BixReader reader)
    {
      base.Deserialize(reader);
      CommandText = reader.ReadString();
    }

    protected internal override void Serialize(BixWriter writer)
    {
      base.Serialize(writer);
      writer.Write(CommandText);
    }
  }
}
