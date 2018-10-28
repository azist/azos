/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Data.Access.MongoDb.Connector;


namespace Azos.Data.Access.MongoDb
{
    /// <summary>
    /// Provides query execution environment in MongoDB database context
    /// </summary>
    public struct MongoDbCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
       public MongoDbCRUDQueryExecutionContext(MongoDbDataStore  store, Database db)
       {
            DataStore = store;
            Database = db;
       }

       public readonly MongoDbDataStore  DataStore;
       public readonly Database  Database;
    }
}
