/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Access.MongoDb.Connector
{
  /// <summary>
  /// Accesses Mongo DB default singleton client of the app context
  /// </summary>
  public static class MongoClientAccessor
  {
    /// <summary>
    /// Accesses default mongo client singleton of app context - it is lazily created
    /// </summary>
    public static MongoClient GetDefaultMongoClient(this IApplication app)
      => app.NonNull(nameof(app))
            .Singletons
            .GetOrCreate(() =>
            {
              var result = new MongoClient(app, "Default");
              result.Configure(null);
              return result;
            }).instance;

    /// <summary>
    /// Return a Database instance per supplied connection string in the following form:
    /// <code>
    /// mongo{server="mongo://localhost:27017" db="myDB"}
    /// </code>
    /// </summary>
    public static Database GetMongoDatabaseFromConnectString(this IApplication app, string cString, string dfltDbName = null)
    {
      if (cString.IsNullOrWhiteSpace())
        throw new MongoDbConnectorException(StringConsts.CONNECTION_STRING_NULL_OR_EMPTY_ERROR);

      var root = cString.AsLaconicConfig();
      if (root == null || !root.IsSameName(MongoClient.CONFIG_CS_ROOT_SECTION))
        throw new MongoDbConnectorException(StringConsts.CONNECTION_STRING_INVALID_ERROR.Args(cString, "Unparsable or not '{0}' root".Args(MongoClient.CONFIG_CS_ROOT_SECTION)));

      var cs = root.AttrByName(MongoClient.CONFIG_CS_SERVER_ATTR).Value;
      var dbn = root.AttrByName(MongoClient.CONFIG_CS_DB_ATTR).Value;
      if (cs.IsNullOrWhiteSpace() || (dbn.IsNullOrWhiteSpace() && dfltDbName.IsNullOrWhiteSpace()))
        throw new MongoDbConnectorException(StringConsts.CONNECTION_STRING_INVALID_ERROR.Args(cString, "Missing attr '{0}' or '{1}'".Args(MongoClient.CONFIG_CS_SERVER_ATTR, MongoClient.CONFIG_CS_DB_ATTR)));


      var client = app.GetDefaultMongoClient();
      var server = client[new Glue.Node(cs)];
      var database = server[dbn.Default(dfltDbName)];
      return database;
    }

  }
}
