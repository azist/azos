/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Conf;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Instrumentation;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Gets role data
  /// </summary>
  public sealed class GetRole : IdCmdBase
  {
    public GetRole(MinIdpMongoDbStore mongo) : base(mongo) { }


    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Get role data
```
  GetRole
  {
    realm='realm' //atom
    id='roleId' //string
  }
```");

    protected override object ExecuteBody()
    {
      var bson = Context.Access((tx) => {
        var crole = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_ROLE)];

       return crole.FindOne(Query.ID_EQ_String(this.Id));
      });

      if (bson == null) return null;

      return new JsonDataMap
      {
        {nameof(SetRole.Id),       bson[BsonDataModel._ID]?.ObjectValue},
        {nameof(SetRole.Rights),    bson[BsonDataModel.FLD_RIGHTS]?.ObjectValue},
        {"createUtc",               bson[BsonDataModel.FLD_CREATEUTC]?.ObjectValue}
      };
    }

  }
}
