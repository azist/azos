/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.Serialization.BSON;
using Azos.Web;

namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Sets role data
  /// </summary>
  public sealed class SetRole : IdCmdBase
  {
    public SetRole(MinIdpMongoDbStore mongo) : base(mongo) { }

    [Config]
    public IConfigSectionNode Rights { get; set; }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Sets role data
```
  SetRole
  {
    realm='realm' //atom
    id='roleId' //string
    rights      //config vector
    {
      ns{ perm{ ... } ...}
      ...
    }
  }
```");

    protected override void Validate()
    {
      base.Validate();
      if (Rights==null || !Rights.Exists) throw new SecurityException("Missing `$rights`") { Code = -1000 };
    }

    protected override object ExecuteBody()
    {
      Context.Access((tx) => {
        var crole = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_ROLE)];
        var role = new BSONDocument();
        role.Set(new BSONStringElement(BsonDataModel._ID, Id));
        role.Set(new BSONStringElement(BsonDataModel.FLD_RIGHTS, Rights.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)));
        var cr = crole.Save(role);
        Aver.IsNull(cr.WriteErrors, cr.WriteErrors.First().Message);
        return cr;
      });
      return "OK";
    }

  }
}
