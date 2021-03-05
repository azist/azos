/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Glue;
using Azos.Instrumentation;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Web;


namespace Azos.Data.Access.MongoDb.Connector.Instrumentation
{
  /// <summary>
  /// Lists server connections
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.VIEW)]
  public sealed class ListConnections : ServerCommand
  {
    public ListConnections(MongoClient mongo) : base(mongo) { }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Lists mongo connections

The command lists all connections per server node

```
ListConnections
{
  srv='mongo://127.0.0.1:27017'
}
```
");

    public override ExternalCallResponse Execute()
    {
      var server = Context[new Node(Server.NonBlank(nameof(Server)))];

      var cnns = server.ConnectionInfos
                       .Select(c => new { id = c.id, busy = c.busy, utcStart = c.sd, utcExpirStart = c.ed })
                       .ToArray();

      var result = new
      {
        node = server.Node,
        appliance = server.Appliance,
        connections = cnns
      };

      var json = result.ToJson(JsonWritingOptions.PrettyPrintASCII);
      return new ExternalCallResponse(ContentType.JSON, json);
    }

  }
}
