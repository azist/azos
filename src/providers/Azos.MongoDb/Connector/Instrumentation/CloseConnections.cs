/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Web;

using System.Linq;

namespace Azos.Data.Access.MongoDb.Connector.Instrumentation
{
  /// <summary>
  /// Closes server connections
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.VIEW_CHANGE)]
  public sealed class CloseConnections : ExternalCallRequest<MongoClient>
  {
    public CloseConnections(MongoClient mongo) : base(mongo) { }

    [Config("$s|$srv|$svr|$server|$node|$uri|$host")]
    public string Server { get; set; }

    [Config]
    public bool Block { get; set; }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Closes mongo connections

The command tries to close all connections per server node,
optionally blocking until all connections are closed

```
CloseConnections
{
  srv='mongo://127.0.0.1:27017'
  block=false
}
```
");

    public override ExternalCallResponse Execute()
    {
      var server = Context[new Node(Server)];

      var before = server.ConnectionInfos.ToArray();

      server.CloseAllConnections(Block);

      var after = server.ConnectionInfos.ToArray();

      var result = new
      {
        node = server.Node,
        appliance = server.Appliance,
        before = before.Length,
        after = after.Length,
        change = after.Length - before.Length,
        open = after.Select(c => new{ id = c.id, busy = c.busy, utcStart =  c.sd, utcExpirStart = c.ed})
      };

      var json = result.ToJson(JsonWritingOptions.PrettyPrintASCII);
      return new ExternalCallResponse(ContentType.JSON, json);
    }

  }
}
