﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Data.Access.MongoDb.Connector;
using Azos.Instrumentation;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Lists all users
  /// </summary>
  public sealed class ListUsers : CmdBase
  {
    public ListUsers(MinIdpMongoDbStore mongo) : base(mongo) { }


    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Lists all users
```
  ListUsers
  {
    realm='realm' //atom
  }
```");

    protected override object ExecuteBody()
    {
      var bson = Context.Access((tx) => {
        var crole = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_USER)];

        return crole.FindAndFetchAll(new Query());
      });

      return bson.Select(doc => {
        var result = new JsonDataMap();
        doc.ForEach(elm => result[elm.Name] = elm.ObjectValue);
        return result;
      });
    }

  }
}
