/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Security.Permissions.Data
{
  /// <summary>
  /// Defines access levels for conf forest tree access
  /// </summary>
  public enum DataRpcAccessLevel
  {
    Denied = AccessLevel.DENIED,
    Read   = AccessLevel.VIEW,
    Transact = AccessLevel.VIEW_CHANGE_DELETE
  }

  /// <summary>
  /// Grants the assignee to have desired access level to execute Data RPC
  /// </summary>
  public sealed class DataRpcPermission : DataContextualPermission
  {
    public DataRpcPermission(DataRpcAccessLevel level) : base((int)level) { }

    public override string Description => "Grants the assignee to have desired access level to execute Data RPC";
  }
}
