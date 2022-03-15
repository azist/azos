/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.AuthKit.Server;
using Azos.Data.Access.MySql;

namespace Azos.AuthKit.Server.MySql
{
  /// <summary>
  /// Declares a MySql data store for AuthKit IDP data.
  /// The class provides services for IdpUserCoreCrudDataLogic which is based on CRUD data access layer
  /// </summary>
  public sealed class MySqlUserStore : MySqlCrudDataStoreBase
  {
    public MySqlUserStore(IdpUserCoreCrudDataLogic director) : base(director) { }

    public override string TargetName     { get => "mysql"; }
    public override bool   FullGDIDS      { get => true; set { } }
    public override bool   StringBool     { get => true; set { } }
    public override string StringForFalse { get => "F"; set { } }
    public override string StringForTrue  { get => "T"; set { } }
  }
}
