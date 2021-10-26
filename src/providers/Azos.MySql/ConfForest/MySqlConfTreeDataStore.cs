/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data.Access.MySql;
using Azos.Conf.Forest.Server;

namespace Azos.MySql.ConfForest
{
  /// <summary>
  /// Declares a MySql data store for config forest tree.
  /// This class is allocated by ForestDataSource
  /// </summary>
  public sealed class MySqlConfForestTreeDataStore : MySqlCrudDataStoreBase
  {
    public MySqlConfForestTreeDataStore(IForestDataSource director) : base(director) { }

    public override string TargetName     { get => "mysql"; }
    public override bool   FullGDIDS      { get => true; set {} }
    public override bool   StringBool     { get => true; set {} }
    public override string StringForFalse { get => "F";  set {} }
    public override string StringForTrue  { get => "T";  set {} }
  }
}
