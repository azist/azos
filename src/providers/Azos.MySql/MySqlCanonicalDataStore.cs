/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Implements MySql CRUD data store that works with canonical data model (using GDIDs and other intrinsics),
  /// auto-generates SQLs for record models and supports CRUD operations.
  /// This class IS thread-safe load/save/delete operations
  /// </summary>
  public class MySqlCanonicalDataStore : MySqlCrudDataStoreBase
  {
    #region .ctor/.dctor
    public MySqlCanonicalDataStore(IApplication app) : base(app) { }
    public MySqlCanonicalDataStore(IApplication app, string cs) : base(app, cs) { }
    public MySqlCanonicalDataStore(IApplicationComponent director) : base(director) { }
    public MySqlCanonicalDataStore(IApplicationComponent director, string cs) : base(director, cs) { }
    #endregion

    #region Protected + Overrides

    /// <summary>
    /// Performs CRUD batch save. Override to do custom batch saving
    /// </summary>
    protected internal async override Task<int> DoSaveAsync(MySqlConnection cnn, MySqlTransaction transaction, RowsetBase[] rowsets)
    {
      if (rowsets == null) return 0;

      var affected = 0;

      foreach (var rset in rowsets)
      {
        foreach (var change in rset.Changes)
        {
          switch (change.ChangeType)
          {
            case DocChangeType.Insert: affected += await DoInsertAsync(cnn, transaction, change.Doc).ConfigureAwait(false); break;
            case DocChangeType.Update: affected += await DoUpdateAsync(cnn, transaction, change.Doc, change.Key).ConfigureAwait(false); break;
            case DocChangeType.Upsert: affected += await DoUpsertAsync(cnn, transaction, change.Doc).ConfigureAwait(false); break;
            case DocChangeType.Delete: affected += await DoDeleteAsync(cnn, transaction, change.Doc, change.Key).ConfigureAwait(false); break;
          }
        }
      }

      return affected;
    }

    /// <summary>
    /// Performs CRUD row insert. Override to do custom insertion
    /// </summary>
    protected internal async override Task<int> DoInsertAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, FieldFilterFunc filter = null)
    {
      CheckReadOnly(row.Schema, "insert");
      return await CrudGenerator.CRUDInsert(this, cnn, transaction, row, filter).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs CRUD row upsert. Override to do custom upsertion
    /// </summary>
    protected internal async override Task<int> DoUpsertAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, FieldFilterFunc filter = null)
    {
      CheckReadOnly(row.Schema, "upsert");
      return await CrudGenerator.CRUDUpsert(this, cnn, transaction, row, filter).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs CRUD row update. Override to do custom update
    /// </summary>
    protected internal async override Task<int> DoUpdateAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      CheckReadOnly(row.Schema, "update");
      return await CrudGenerator.CRUDUpdate(this, cnn, transaction, row, key, filter).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs CRUD row deletion. Override to do custom deletion
    /// </summary>
    protected internal async override Task<int> DoDeleteAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, IDataStoreKey key = null)
    {
      CheckReadOnly(row.Schema, "delete");
      return await CrudGenerator.CRUDDelete(this, cnn, transaction, row, key).ConfigureAwait(false);
    }
    #endregion
  }
}
