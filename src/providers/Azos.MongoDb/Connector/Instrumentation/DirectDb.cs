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
  public sealed class DirectDb : ExternalCallRequest<MongoClient>
  {
    public DirectDb(MongoClient mongo) : base(mongo) { }

    [Config("$s|$srv|$svr|$server|$node|$uri|$host")]
    public string Server { get; set; }

    [Config("$d|$db|$data|$database")]
    public string Database { get; set; }

    [Config("$c|$cmd|$command")]
    public string Command { get; set; }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"Pipes Mongo JSON command directly into database.
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
