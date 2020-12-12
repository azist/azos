/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
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

    [Config("rights")] public IConfigSectionNode Rights { get; set; }
    [Config] public string Acl { get; set; }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Sets role data
```
  SetRole
  {
    realm='realm' //atom
    id='roleId' //string
    rights      //config vector or use acl
    {
      ns{ perm{ ... } ...}
      ...
    }
    // OR use ACL string attribute
    acl='...laconic acl...' //config string ACL may be use in place of rights
  }
```");

    protected override void Validate()
    {
      base.Validate();

      if (Acl.IsNotNullOrWhiteSpace())
      {
        if (Rights != null && Rights.Exists)
          throw new CallGuardException(GetType().Name, "Acl", "Must use either `rights{}` or `$acl`") { Code = -1000 };

        try
        {
          Rights = Acl.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
        }
        catch(Exception error)
        {
          throw new CallGuardException(GetType().Name, "Acl", "Bad `$acl`: " + error.ToMessageWithType(), error) { Code = -1000 };
        }
      }

      if (Rights==null || !Rights.Exists) throw new CallGuardException(GetType().Name, "Rights", "Missing `rights{}`") { Code = -1000 };
    }

    protected override object ExecuteBody()
    {
      var crud = Context.Access((tx) => {
        var crole = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_ROLE)];
        var role = new BSONDocument();
        role.Set(new BSONStringElement(BsonDataModel._ID, Id));
        role.Set(new BSONStringElement(BsonDataModel.FLD_RIGHTS, Rights.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)));
        role.Set(new BSONDateTimeElement(BsonDataModel.FLD_CREATEUTC, Context.App.TimeSource.UTCNow));
        var cr = crole.Save(role);
        Aver.IsNull(cr.WriteErrors, cr.WriteErrors?.FirstOrDefault().Message);
        return cr;
      });

      return crud;
    }

  }
}
