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
  /// Decorates package commands which get written into package archives during packing -
  /// and read back during unpacking/installation activities
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class PackageCommandAttribute : GuidTypeAttribute
  {
    public PackageCommandAttribute(string typeGuid) : base(typeGuid) { }
  }

  /// <summary>
  /// Base for commands which are stored in package archive stream
  /// </summary>
  public abstract class Command
  {
    public string Condition { get; set; }


    /// <summary>
    /// Called by installer, return true if it runs or false if it does not e.g. when condition is not met
    /// </summary>
    public bool Execute(Installer state)//state machine is installer
    {
      state.NonNull(nameof(state));
      if (Condition.IsNotNullOrWhiteSpace())
      {
        //eval condition, return false
      }
      return DoExecute(state);
    }

    protected abstract bool DoExecute(Installer state);

    protected internal virtual void Serialize(BixWriter writer)
    {
      //write itself into BIX
    }

    protected internal virtual void Deserialize(BixReader reader)
    {
      //read itself from BIX
    }
  }


  /// <summary>
  /// Ends installation
  /// </summary>
  [PackageCommand("b3e0130c-3a51-4e8d-b91c-afa86ab1c323")]
  public sealed class EndCommand : Command
  {

    public string ScriptText { get; set; }

    protected override bool DoExecute(Installer state)
    {
      throw new NotImplementedException();
    }
  }


  /// <summary>
  /// Executes script on target host
  /// </summary>
  [PackageCommand("90b71e1c-8aa8-422d-8213-c19414de845f")]
  public sealed class ExecCommand : Command
  {

    public string ScriptText{  get; set; }

    protected override bool DoExecute(Installer state)
    {
      throw new NotImplementedException();
    }
  }


  /// <summary>
  /// Creates directory
  /// </summary>
  [PackageCommand("99eeaaf7-3298-47e8-83bf-0f254a775a77")]
  public sealed class CreateDirCommand : Command
  {
    protected override bool DoExecute(Installer state)
    {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  ///  Creates file
  /// </summary>
  [PackageCommand("b95712f5-9a69-49e5-b848-999b206e815f")]
  public sealed class CreateFileCommand : Command
  {
    protected override bool DoExecute(Installer state)
    {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  ///  Closes file being created
  /// </summary>
  [PackageCommand("33b9e876-d918-442e-9a64-ce5067cf0445")]
  public sealed class CloseFileCommand : Command
  {
    protected override bool DoExecute(Installer state)
    {
      throw new NotImplementedException();
    }
  }

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

    protected override bool DoExecute(Installer state)
    {
      throw new NotImplementedException();
    }

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
