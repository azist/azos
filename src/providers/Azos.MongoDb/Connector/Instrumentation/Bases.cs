/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Access.MongoDb.Connector.Instrumentation
{
  public abstract class ServerCommand : ExternalCallRequest<MongoClient>
  {
    public ServerCommand(MongoClient mongo) : base(mongo) { }

    [Config("$s|$srv|$svr|$server|$node|$uri|$host")]
    public string Server { get; set; }
  }
}
