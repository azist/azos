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
