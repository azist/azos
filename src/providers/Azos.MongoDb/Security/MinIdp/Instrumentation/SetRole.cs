/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Data.Access.MongoDb.Client;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Security;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;
using Azos.Web;


namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Sets role data
  /// </summary>
  public sealed class SetRole : CmdBase
  {
    public SetRole(MinIdpMongoDbStore mongo) : base(mongo) { }

    [Config]
    public IConfigSectionNode Rights { get; set; }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"Sets Role data");

    protected override object ExecuteBody()
    {
      Context.Access((tx) => tx.Db);
      return "";
    }

  }
}
