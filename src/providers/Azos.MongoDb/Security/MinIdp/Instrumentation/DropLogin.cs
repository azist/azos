/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Data.Access.MongoDb.Connector;
using Azos.Instrumentation;
using Azos.Web;

namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Deletes login
  /// </summary>
  public sealed class DropLogin : IdCmdBase
  {
    public DropLogin(MinIdpMongoDbStore mongo) : base(mongo) { }


    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Deletes login
```
  DropLogin
  {
    realm='realm' //atom
    id='login' //string
  }
```");

    protected override object ExecuteBody()
    {
      var crud = Context.Access((tx) => {
        var crole = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_LOGIN)];

        var cr = crole.DeleteOne(Query.ID_EQ_String(this.Id));
        Aver.IsNull(cr.WriteErrors, cr.WriteErrors?.FirstOrDefault().Message);
        return cr;
      });

      return crud;
    }

  }
}
