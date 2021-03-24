/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Conf;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Instrumentation;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Gets login data
  /// </summary>
  public sealed class GetLogin : IdCmdBase
  {
    public GetLogin(MinIdpMongoDbStore mongo) : base(mongo) { }


    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Get login data
```
  GetLogin
  {
    realm='realm' //atom
    login='login' //string
  }
```");

    protected override object ExecuteBody()
    {
      var bson = Context.Access((tx) => {
        var crole = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_LOGIN)];

       return crole.FindOne(Query.ID_EQ_String(this.Id));
      });

      if (bson==null) return null;

      return new JsonDataMap
      {
        {nameof(SetLogin.Id),       bson[BsonDataModel._ID]?.ObjectValue},
        {nameof(SetLogin.SysId),    bson[BsonDataModel.FLD_SYSID]?.ObjectValue},
        {nameof(SetLogin.Password), "*******"},
        {"createUtc",               bson[BsonDataModel.FLD_CREATEUTC]?.ObjectValue},
        {nameof(SetLogin.StartUtc), bson[BsonDataModel.FLD_STARTUTC]?.ObjectValue},
        {nameof(SetLogin.EndUtc),   bson[BsonDataModel.FLD_ENDUTC]?.ObjectValue},
      };
    }

  }
}
