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
    /// <summary>
    /// Add optional condition expression which evaluates during execution.
    /// If it passes then the command body is executed, otherwise skipped
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// Optionally describes what command does
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Called by installer, return true if it runs or false if it does not e.g. when condition is not met
    /// </summary>
    public bool Execute(Installer state)//state machine is installer
    {
      state.NonNull(nameof(state));

      if (Condition.IsNotNullOrWhiteSpace())
      {
        var pass = state.EvaluateCondition(Condition);
        if (!pass) return false;
      }

      DoExecute(state);
      return true;
    }

    protected abstract void DoExecute(Installer state);

    protected internal virtual void Serialize(BixWriter writer)
    {
      writer.Write(Condition);
      writer.Write(Description);
    }

    protected internal virtual void Deserialize(BixReader reader)
    {
      Condition = reader.ReadString();
      Description = reader.ReadString();
    }
  }
}
