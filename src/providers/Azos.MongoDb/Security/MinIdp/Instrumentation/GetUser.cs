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
  /// Gets user data
  /// </summary>
  public sealed class GetUser : LongIdCmdBase
  {
    public GetUser(MinIdpMongoDbStore mongo) : base(mongo) { }


    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Get user data
```
  GetUser
  {
    realm='realm' //atom
    id=123 //long
  }
```");

    protected override object ExecuteBody()
    {
      var bson = Context.Access((tx) => {
        var crole = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_USER)];

       return crole.FindOne(Query.ID_EQ_Int64(this.Id));
      });

      return bson.ToJson(JsonWritingOptions.PrettyPrintASCII);
    }

  }
}
