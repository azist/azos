/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;

namespace Azos.Sky.Security.Permissions.Chronicle
{
  /// <summary>
  /// Controls the access level to chronicle service
  /// </summary>
  public enum ChronicleAccessLevel
  {
    Denied  = AccessLevel.DENIED,

    /// <summary>
    /// Can emit (write) messages, basically using the service as a client.
    /// Most clients never go above this level as they should not be allowed to browse chronicles emitted
    /// by other applications in the enterprise
    /// </summary>
    Emit    = AccessLevel.VIEW,

    /// <summary>
    /// Can browse the written chronicles, this is HIGHER access level than Emit as it allows the bearer to browse
    /// what has been written by many different applications. Use Caution granting this level
    /// </summary>
    Browse  = AccessLevel.VIEW_CHANGE,

    /// <summary>
    /// Can also archive (initiate possible deletion) of old data, this is the highest level
    /// </summary>
    Archive = AccessLevel.VIEW_CHANGE_DELETE
  }

  /// <summary>
  /// Controls whether users can access chronicle functionality
  /// </summary>
  public sealed class ChroniclePermission : TypedPermission
  {
    public ChroniclePermission() : this(ChronicleAccessLevel.Emit) { }

    public ChroniclePermission(ChronicleAccessLevel level) : base((int)level) { }

    public override string Description => StringConsts.PERMISSION_DESCRIPTION_ChroniclePermission;
  }
}
