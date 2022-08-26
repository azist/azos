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
  /// Starts a named target span as of this place in the package stream.
  /// All subsequent commands are of this target name, until another similar command
  /// changes the target name to something else
  /// </summary>
  [PackageCommand("d538d2e7-e53b-4cf7-9173-e27fc2b5b02c")]
  public sealed class StartTargetCommand : Command
  {
    public string TargetName { get; set; }

    protected override void DoExecute(Installer state)
      => throw new NotSupportedException($"Explicit execution of {nameof(StartTargetCommand)} is prohibited");

    protected internal override void Serialize(BixWriter writer)
    {
      base.Serialize(writer);
      writer.Write(TargetName);
    }

    protected internal override void Deserialize(BixReader reader)
    {
      base.Deserialize(reader);
      TargetName = reader.ReadString();
    }
  }
}
