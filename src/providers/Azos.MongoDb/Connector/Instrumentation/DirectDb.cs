/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Security;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;
using Azos.Web;


namespace Azos.Data.Access.MongoDb.Connector.Instrumentation
{
  /// <summary>
  /// Provides direct Mongo command execution capability
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public sealed class DirectDb : ServerCommand
  {
    public DirectDb(MongoClient mongo) : base(mongo) { }

    [Config("$d|$db|$data|$database")]
    public string Database { get; set; }

    [Config("$c|$cmd|$command")]
    public string Command { get; set; }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Pipes Mongo JSON command directly into database.

Examples:

```
//1 - Full manc command to get rec count from sky chronicle:
$ manc{sid=XXX call{  directdb{ s='mongo://localhost:27017' d='sky_chron'  cmd='{count: ""sky_log""}'}  }}

//2 - Get record count
DirectDb
{
  s='mongo://127.0.0.1:27017'
  d='mydb'
  cmd='{count: ""patient""}' // count documents
}

//3 - Find a few records
DirectDb
{
  srv='mongo://127.0.0.1:27017'
  db='mydb'
  cmd='{find: ""doctor"", limit: 3}' //find top 3
}

//4 - Create indexes on collection 'log'
DirectDb
{
  srv='mongo://127.0.0.1:27017'
  db='mydb'
  cmd=$'{createIndexes: ""log"", indexes: [
    {
      key: {u: 1},      //list of document fields
      name: ""idx_utc"",  //name of index
      unique: false       //not unique
    }
  ]}'
}
```

For the list of commands see:
 https://docs.mongodb.com/manual/reference/command/find/
 https://docs.mongodb.com/manual/reference/command/delete/
 https://docs.mongodb.com/manual/reference/command/createIndexes/");

    public override ExternalCallResponse Execute()
    {
      var request = new BSONDocument(Command, false);
      var server = Context[new Node(Server)];
      var db = server[Database];
      var response = db.RunCommand(request);

      var json = response.ToJson(JsonWritingOptions.PrettyPrintASCII);
      return new ExternalCallResponse(ContentType.JSON, json);
    }

  }
}
